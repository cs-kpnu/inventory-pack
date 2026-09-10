namespace InventoryPack.Data.Entities;

/// <summary>
///     Stores ledger rows that failed parsing or validation during import.
/// </summary>
public class RejectedRow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int RowNumber { get; set; }
    public required string RawText { get; set; }
    public RejectionReason Reason { get; set; }
    public string? Subaccount { get; set; }
    public string? Mvo { get; set; }
}