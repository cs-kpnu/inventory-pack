using InventoryPack.Data.Entities;

namespace InventoryPack.Features.Assets.Import;

public static class AssetImportValidator
{
    public static ImportResult Validate(IEnumerable<ParsedAssetRow> rows)
    {
        var candidates = new List<ParsedAssetRow>();
        var rejected = new List<RejectedRow>();

        foreach (var row in rows)
            if (GetRejectionReason(row) is { } reason)
                rejected.Add(CreateRejected(row, reason));
            else
                candidates.Add(row);

        var duplicateCodes = candidates
            .SelectMany(r => r.InventoryNumbers)
            .GroupBy(c => c, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var ledgerEntries = new List<LedgerEntry>();
        foreach (var row in candidates)
            if (row.InventoryNumbers.Any(duplicateCodes.Contains))
                rejected.Add(CreateRejected(row, RejectionReason.DuplicateCode));
            else
                ledgerEntries.Add(CreateLedgerEntry(row));

        return new ImportResult(ledgerEntries, rejected);
    }

    private static LedgerEntry CreateLedgerEntry(ParsedAssetRow row)
    {
        var qty = (int)row.Quantity!;
        var entry = new LedgerEntry
        {
            Name = row.Name ?? string.Empty,
            Mvo = row.Mvo ?? string.Empty,
            Subaccount = row.Subaccount ?? string.Empty,
            Quantity = qty
        };

        var isItemized = row.InventoryNumbers.Count == qty;
        for (var i = 0; i < qty; i++)
            entry.Assets.Add(new Asset
            {
                InventoryNumber = isItemized ? row.InventoryNumbers[i] : row.InventoryNumbers[0],
                UnitIndex = i + 1,
                LedgerEntry = entry
            });

        return entry;
    }

    private static RejectedRow CreateRejected(ParsedAssetRow row, RejectionReason reason)
    {
        return new RejectedRow
        {
            RowNumber = row.RowNumber,
            RawText = row.RawText,
            Reason = reason,
            Mvo = row.Mvo,
            Subaccount = row.Subaccount
        };
    }

    private static RejectionReason? GetRejectionReason(ParsedAssetRow row)
    {
        if (string.IsNullOrEmpty(row.Mvo) || string.IsNullOrEmpty(row.Subaccount))
            return RejectionReason.MissingContext;

        if (row.Quantity is null or <= 0 || row.Quantity % 1 != 0)
            return RejectionReason.InvalidQuantity;

        if (row.InventoryNumbers.Count == 0)
            return RejectionReason.NoSingleCode;

        if (row.InventoryNumbers.Count > 1 && row.InventoryNumbers.Count != (int)row.Quantity)
            return RejectionReason.CodeQuantityMismatch;

        return null;
    }
}