namespace InventoryPack.Data.Entities;

public class RejectedRowReason
{
    public Guid RejectedRowId { get; set; }
    public RejectedRow RejectedRow { get; set; } = null!;
    public RejectionReason Reason { get; set; }
}