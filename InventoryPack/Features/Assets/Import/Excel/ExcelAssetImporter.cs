namespace InventoryPack.Features.Assets.Import.Excel;

public class ExcelAssetImporter : IAssetImporter
{
    public ImportResult Import(Stream stream)
    {
        return ExcelAssetParser.Parse(stream);
    }
}