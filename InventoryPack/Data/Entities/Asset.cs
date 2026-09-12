namespace InventoryPack.Data.Entities;

/// <summary>
///     Represents a single physical inventory asset.
/// </summary>
public class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LedgerEntryId { get; set; }
    public LedgerEntry LedgerEntry { get; set; } = null!;
    public required string InventoryNumber { get; set; }
    public int UnitIndex { get; set; } = 1;
    public DateTime? PrintedAt { get; set; }
}