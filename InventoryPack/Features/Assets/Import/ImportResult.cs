using InventoryPack.Data.Entities;

namespace InventoryPack.Features.Assets.Import;

/// <summary>
///     Represents the result of an import operation.
/// </summary>
public record ImportResult(
    IReadOnlyList<LedgerEntry> LedgerEntries,
    IReadOnlyList<RejectedRow> RejectedRows
)
{
    public int TotalAssets => LedgerEntries.Sum(e => e.Assets.Count);
}