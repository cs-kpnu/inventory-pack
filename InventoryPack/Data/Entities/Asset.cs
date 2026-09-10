namespace InventoryPack.Data.Entities;

/// <summary>
///     Represents a single physical inventory asset.
/// </summary>
public class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string InventoryNumber { get; set; }
    public required string Name { get; set; }
    public required string Mvo { get; set; }
    public required string Subaccount { get; set; }
    public DateTime? PrintedAt { get; set; }
}