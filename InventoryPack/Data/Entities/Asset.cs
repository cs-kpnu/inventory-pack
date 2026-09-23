namespace InventoryPack.Data.Entities;

/// <summary>
///     Represents a single physical inventory asset with a permanent identity.
/// </summary>
public class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CodeGroupId { get; set; }
    public CodeGroup CodeGroup { get; set; } = null!;

    public Guid? RequestId { get; set; }

    public string? RegisteredForMvo { get; set; }

    public Guid? RegisteredFromLedgerEntryId { get; set; }

    public DateTime AllocatedAt { get; set; } = DateTime.UtcNow;
}
