namespace InventoryPack.Data.Entities;

/// <summary>One detected issue, shared by all affected source rows.</summary>
public class RejectionIssue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public RejectionReason Reason { get; set; }
    public string? Code { get; set; }
    public List<RejectedRowIssue> Rows { get; set; } = [];
}
