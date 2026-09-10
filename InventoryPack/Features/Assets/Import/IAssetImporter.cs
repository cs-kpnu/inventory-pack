namespace InventoryPack.Features.Assets.Import;

/// <summary>
///     Abstraction for parsing raw asset records from various file formats.
/// </summary>
public interface IAssetImporter
{
    IReadOnlyList<ParsedAssetRow> Parse(Stream stream);
}