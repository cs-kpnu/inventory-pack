namespace InventoryPack.Data.Entities;

public class RejectedRowIssue
{
    public Guid RejectedRowId { get; set; }
    public RejectedRow RejectedRow { get; set; } = null!;
    public Guid RejectionIssueId { get; set; }
    public RejectionIssue RejectionIssue { get; set; } = null!;
}
