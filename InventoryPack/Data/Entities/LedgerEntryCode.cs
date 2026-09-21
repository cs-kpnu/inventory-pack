namespace InventoryPack.Data.Entities;

public class LedgerEntryCode
{
    public Guid LedgerEntryId { get; set; }
    public LedgerEntry LedgerEntry { get; set; } = null!;

    public Guid CodeGroupId { get; set; }
    public CodeGroup CodeGroup { get; set; } = null!;
}