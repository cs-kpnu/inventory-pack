namespace InventoryPack.Data.Entities;

public class LedgerEntryCode
{
    public Guid LedgerEntryId { get; set; }
    public LedgerEntry LedgerEntry { get; set; } = null!;
    public required string Code { get; set; }
}