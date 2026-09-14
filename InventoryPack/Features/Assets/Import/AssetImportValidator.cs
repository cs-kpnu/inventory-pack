using InventoryPack.Data.Entities;

namespace InventoryPack.Features.Assets.Import;

public static class AssetImportValidator
{
    public static ImportResult Validate(IEnumerable<ParsedAssetRow> rows)
    {
        var candidates = new List<ParsedAssetRow>();
        var rejected = new List<RejectedRow>();

        foreach (var row in rows)
        {
            var reasons = GetRejectionReasons(row);
            if (reasons.Count > 0)
                rejected.Add(CreateRejected(row, reasons));
            else
                candidates.Add(row);
        }

        var duplicateCodes = candidates
            .SelectMany(r => r.InventoryNumbers)
            .GroupBy(c => c, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var ledgerEntries = new List<LedgerEntry>();
        foreach (var row in candidates)
            if (row.InventoryNumbers.Any(duplicateCodes.Contains))
                rejected.Add(CreateRejected(row, [RejectionReason.DuplicateCode]));
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

    private static RejectedRow CreateRejected(ParsedAssetRow row, IEnumerable<RejectionReason> reasons)
    {
        var rejected = new RejectedRow
        {
            RowNumber = row.RowNumber,
            RawText = row.RawText,
            Mvo = row.Mvo,
            Subaccount = row.Subaccount,
            Quantity = row.Quantity
        };

        foreach (var reason in reasons.Distinct())
            rejected.Reasons.Add(new RejectedRowReason
            {
                Reason = reason,
                RejectedRow = rejected
            });

        return rejected;
    }

    private static List<RejectionReason> GetRejectionReasons(ParsedAssetRow row)
    {
        var reasons = new List<RejectionReason>();

        if (string.IsNullOrEmpty(row.Mvo) || string.IsNullOrEmpty(row.Subaccount))
            reasons.Add(RejectionReason.MissingContext);

        if (row.Quantity is null or <= 0 || row.Quantity % 1 != 0)
            reasons.Add(RejectionReason.InvalidQuantity);

        if (row.InventoryNumbers.Count == 0)
            reasons.Add(RejectionReason.NoSingleCode);

        if (row.Quantity is > 0 && row.Quantity % 1 == 0 && row.InventoryNumbers.Count > 1 &&
            row.InventoryNumbers.Count != (int)row.Quantity)
            reasons.Add(RejectionReason.CodeQuantityMismatch);

        return reasons;
    }
}