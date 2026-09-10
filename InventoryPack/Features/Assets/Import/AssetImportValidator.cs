using InventoryPack.Data.Entities;

namespace InventoryPack.Features.Assets.Import;

public static class AssetImportValidator
{
    public static ImportResult Validate(IEnumerable<ParsedAssetRow> rows)
    {
        var (candidates, rejected) = FilterInvalidRows(rows);
        var (assets, duplicateRejections) = ResolveDuplicates(candidates);

        rejected.AddRange(duplicateRejections);

        return new ImportResult(assets, rejected);
    }

    private static (List<ParsedAssetRow> Candidates, List<RejectedRow> Rejected) FilterInvalidRows(
        IEnumerable<ParsedAssetRow> rows)
    {
        var candidates = new List<ParsedAssetRow>();
        var rejected = new List<RejectedRow>();

        foreach (var row in rows)
            if (GetRejectionReason(row) is { } reason)
                rejected.Add(row.ToRejected(reason));
            else
                candidates.Add(row);

        return (candidates, rejected);
    }

    private static (List<Asset> Assets, List<RejectedRow> Duplicates) ResolveDuplicates(
        List<ParsedAssetRow> candidates)
    {
        var assets = new List<Asset>();
        var duplicates = new List<RejectedRow>();

        foreach (var group in candidates.GroupBy(r => r.InventoryNumber!, StringComparer.OrdinalIgnoreCase))
            if (group.Count() == 1)
                assets.Add(group.First().ToAsset());
            else
                duplicates.AddRange(group.Select(r => r.ToRejected(RejectionReason.DuplicateCode)));

        return (assets, duplicates);
    }

    private static RejectionReason? GetRejectionReason(ParsedAssetRow row)
    {
        return row switch
        {
            { Quantity: not 1 } => RejectionReason.MultipleQuantity,
            { Mvo: null or "" } or { Subaccount: null or "" } => RejectionReason.MissingContext,
            { InventoryNumber: null or "" } => RejectionReason.NoSingleCode,
            _ => null
        };
    }
}