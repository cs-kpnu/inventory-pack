using InventoryPack.Data.Entities;

namespace InventoryPack.Features.Assets.Import;

/// <summary>
///     Represents the result of an import operation.
/// </summary>
public record ImportResult(
    IReadOnlyList<ValidatedAssetRow> ValidatedRows,
    IReadOnlyList<RejectedRow> RejectedRows
);