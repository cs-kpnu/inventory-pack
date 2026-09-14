namespace InventoryPack.Features.Assets.Import;

/// <summary>
///     Intermediate representation of a parsed row extracted from an import file.
/// </summary>
public record ParsedAssetRow(
    int RowNumber,
    string RawText,
    string? Name,
    IReadOnlyList<string> InventoryNumbers,
    string? Mvo,
    string? Subaccount,
    decimal? Quantity
);