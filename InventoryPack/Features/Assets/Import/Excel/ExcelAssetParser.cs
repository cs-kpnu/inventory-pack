using System.Text.RegularExpressions;
using MiniExcelLibs;

namespace InventoryPack.Features.Assets.Import.Excel;

public static partial class ExcelAssetParser
{
    [GeneratedRegex(@"^(.*?)\s*\(\s*([^)]+?)\s*\)\s*$", RegexOptions.Singleline)]
    private static partial Regex TrailingParenthesesRegex();

    [GeneratedRegex(@"\b\d{6,}\b")]
    private static partial Regex CodeRegex();

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

            var title = row.Title!.Trim();
            var match = TrailingParenthesesRegex().Match(title);

            var name = title;
            IReadOnlyList<string> codes = [];

            if (match.Success)
            {
                var extractedCodes = CodeRegex()
                    .Matches(match.Groups[2].Value)
                    .Select(m => m.Value)
                    .ToList();

                if (extractedCodes.Count > 0)
                {
                    name = match.Groups[1].Value.Trim();
                    codes = extractedCodes;
                }
            }

            rows.Add(new ParsedAssetRow(
                rowNumber,
                row.Title!,
                name,
                codes,
                row.Mvo?.Trim(),
                row.Subaccount?.Trim(),
                row.Quantity
            ));
        }

        return rows;
    }
}