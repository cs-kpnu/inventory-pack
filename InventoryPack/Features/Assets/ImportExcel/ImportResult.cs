using InventoryPack.Data.Entities;

namespace InventoryPack.Features.Assets.ImportExcel;

/// <summary>
///     Represents the result of an import operation.
/// </summary>
public record ImportResult(
    IReadOnlyList<Asset> Assets,
    IReadOnlyList<RejectedRow> RejectedRows
);