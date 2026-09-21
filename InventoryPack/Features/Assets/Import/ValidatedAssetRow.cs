namespace InventoryPack.Features.Assets.Import;

/// <summary>
///     Pure representation of a validated source row, independent of database entities.
/// </summary>
public record ValidatedAssetRow(
    int SourceRowNumber,
    string SourceTitle,
    string? Unit,
    string Name,
    string? Mvo,
    string Subaccount,
    decimal Quantity,
    IReadOnlyList<string> InventoryNumbers
);