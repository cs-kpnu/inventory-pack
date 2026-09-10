using System.Text.RegularExpressions;
using MiniExcelLibs;

namespace InventoryPack.Features.Assets.Import.Excel;

public static partial class ExcelAssetParser
{
    [GeneratedRegex(@"^(.*?)\s*\(\s*(\d{6,})\s*\)\s*$")]
    private static partial Regex SingleCodeRegex();

    public static List<ParsedAssetRow> Parse(Stream stream)
    {
        var rows = new List<ParsedAssetRow>();
        var rowNumber = 0;

        foreach (var row in stream.Query<ExcelAssetRow>(hasHeader: false))
        {
            rowNumber++;

            if (row.Quantity is null || string.IsNullOrWhiteSpace(row.Title) || row.Title.StartsWith('-') ||
                row.Title.Contains("Найменування"))
                continue;

            var match = SingleCodeRegex().Match(row.Title!);

            rows.Add(new ParsedAssetRow(
                rowNumber,
                row.Title!,
                match.Success ? match.Groups[1].Value.Trim() : null,
                match.Success ? match.Groups[2].Value : null,
                row.Mvo?.Trim(),
                row.Subaccount?.Trim(),
                row.Quantity
            ));
        }

        return rows;
    }
}