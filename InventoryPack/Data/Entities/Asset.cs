using System.ComponentModel.DataAnnotations;

namespace InventoryPack.Data.Entities;

/// <summary>
///     Represents a single physical inventory asset.
/// </summary>
public class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(32)] public required string InventoryNumber { get; set; }
    [MaxLength(500)] public required string Name { get; set; }
    [MaxLength(150)] public required string Mvo { get; set; }
    [MaxLength(32)] public required string Subaccount { get; set; }
    public DateTime? PrintedAt { get; set; }
}