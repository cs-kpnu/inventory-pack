namespace InventoryPack.Data.Entities;

/// <summary>
///     Represents an accounting ledger row.
/// </summary>
public class LedgerEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int SourceRowNumber { get; set; }
    public string SourceTitle { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public required string Subaccount { get; set; }
    public string? Mvo { get; set; }
    public required string Name { get; set; }
    public decimal Quantity { get; set; }

    public List<LedgerEntryCode> Codes { get; set; } = [];
}