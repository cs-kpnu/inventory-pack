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

    public LedgerEntry ToLedgerEntry()
    {
        var qty = (int)Quantity!;
        var entry = new LedgerEntry
        {
            Name = Name ?? string.Empty,
            Mvo = Mvo ?? string.Empty,
            Subaccount = Subaccount ?? string.Empty,
            Quantity = qty
        };

        var isItemized = InventoryNumbers.Count == qty;
        for (var i = 0; i < qty; i++)
            entry.Assets.Add(new Asset
            {
                InventoryNumber = isItemized ? InventoryNumbers[i] : InventoryNumbers[0],
                UnitIndex = i + 1,
                LedgerEntry = entry
            });

        return entry;
    }
}