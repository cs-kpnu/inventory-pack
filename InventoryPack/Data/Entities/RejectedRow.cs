using System.ComponentModel.DataAnnotations;

namespace InventoryPack.Data.Entities;

/// <summary>
///     Stores ledger rows that failed parsing or validation during import.
/// </summary>
public class RejectedRow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int RowNumber { get; set; }
    [MaxLength(2000)] public required string RawText { get; set; }
    public RejectionReason Reason { get; set; }
    [MaxLength(32)] public string? Subaccount { get; set; }
    [MaxLength(150)] public string? Mvo { get; set; }
}