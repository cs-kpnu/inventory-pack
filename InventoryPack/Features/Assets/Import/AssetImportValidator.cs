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
                rejected.Add(row.ToRejected(reason));
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
                rejected.Add(row.ToRejected(RejectionReason.DuplicateCode));
            else
                ledgerEntries.Add(row.ToLedgerEntry());

        return new ImportResult(ledgerEntries, rejected);
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