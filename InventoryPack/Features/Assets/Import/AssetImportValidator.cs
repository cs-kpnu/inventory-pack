using InventoryPack.Data.Entities;

namespace InventoryPack.Features.Assets.Import;

public static class AssetImportValidator
{
    public static ImportResult Validate(IEnumerable<ParsedAssetRow> rows)
    {
        var assets = new List<Asset>();
        var rejected = new List<RejectedRow>();

        foreach (var row in rows)
            if (GetRejectionReason(row) is { } reason)
                rejected.Add(row.ToRejected(reason));
            else
                assets.Add(row.ToAsset());

        return new ImportResult(assets, rejected);
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