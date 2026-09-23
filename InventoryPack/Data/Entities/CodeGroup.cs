namespace InventoryPack.Data.Entities;

/// <summary>
///     Represents a stable accountant inventory code, independent of individual source rows.
/// </summary>
public class CodeGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Code { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<LedgerEntryCode> LedgerCodes { get; set; } = [];
    public List<Asset> Assets { get; set; } = [];
}
