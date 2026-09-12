using InventoryPack.Data.Entities;

namespace InventoryPack.Features.Assets.Import;

/// <summary>
///     Intermediate representation of a parsed row extracted from an import file.
/// </summary>
public record ParsedAssetRow(
    int RowNumber,
    string RawText,
    string? Name,
    IReadOnlyList<string> InventoryNumbers,
    string? Mvo,
    string? Subaccount,
    decimal? Quantity
)
{
    public string? InventoryNumber => InventoryNumbers.Count > 0 ? InventoryNumbers[0] : null;

    public Asset ToAsset()
    {
        return new Asset
        {
            InventoryNumber = InventoryNumber!,
            LedgerEntry = new LedgerEntry
            {
                Name = Name!,
                Mvo = Mvo!,
                Subaccount = Subaccount!,
                Quantity = (int)(Quantity ?? 1)
            }
        };
    }

    public RejectedRow ToRejected(RejectionReason reason)
    {
        return new RejectedRow
        {
            RowNumber = RowNumber,
            RawText = RawText,
            Reason = reason,
            Mvo = Mvo,
            Subaccount = Subaccount
        };
    }
}