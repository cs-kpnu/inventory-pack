namespace InventoryPack.Features.Assets.Import.Excel;

public class ExcelAssetImporter : IAssetImporter
{
    public IReadOnlyList<ParsedAssetRow> Parse(Stream stream)
    {
        return ExcelAssetParser.Parse(stream);
    }
}