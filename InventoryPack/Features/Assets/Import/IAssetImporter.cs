namespace InventoryPack.Features.Assets.Import;

/// <summary>
///     Abstraction for importing assets from various file formats.
/// </summary>
public interface IAssetImporter
{
    ImportResult Import(Stream stream);
}