namespace InventoryPack.Data.Entities;

/// <summary>
///     Represents an accounting ledger row.
/// </summary>
public class LedgerEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Subaccount { get; set; }
    public required string Mvo { get; set; }
    public required string Name { get; set; }
    public decimal Quantity { get; set; }

    public List<Asset> Assets { get; set; } = [];
}