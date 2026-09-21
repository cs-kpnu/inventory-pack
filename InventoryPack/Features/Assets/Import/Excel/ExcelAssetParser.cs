using MiniExcelLibs;

namespace InventoryPack.Features.Assets.Import.Excel;

public static class ExcelAssetParser
{
    public static bool TryExtractCodeSuffix(
        string title,
        out string name,
        out IReadOnlyList<string> codes)
    {
        name = title;
        codes = [];

        var trimmed = title.Trim();
        if (!trimmed.EndsWith(')'))
            return false;

        var openParenIndex = trimmed.LastIndexOf('(');
        if (openParenIndex < 0)
            return false;

        var suffixContent = trimmed.Substring(openParenIndex + 1, trimmed.Length - openParenIndex - 2);
        if (string.IsNullOrWhiteSpace(suffixContent))
            return false;

        var tokenStrings = suffixContent.Split(',');
        var extractedCodes = new List<string>(tokenStrings.Length);

        foreach (var rawToken in tokenStrings)
        {
            var token = rawToken.Trim();
            if (token.Length is < 6 or > 32)
                return false;

            if (!token.All(char.IsAsciiDigit))
                return false;

            extractedCodes.Add(token);
        }

        if (extractedCodes.Count == 0)
            return false;

        name = trimmed[..openParenIndex].Trim();
        codes = extractedCodes;
        return true;
    }

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
            var name = title;
            IReadOnlyList<string> codes = [];

            if (TryExtractCodeSuffix(title, out var extractedName, out var extractedCodes))
            {
                name = extractedName;
                codes = extractedCodes;
            }

            rows.Add(new ParsedAssetRow(
                rowNumber,
                row.Title!,
                name,
                codes,
                row.Mvo?.Trim(),
                row.Subaccount?.Trim(),
                row.Quantity,
                row.Unit?.Trim()
            ));
        }

        return rows;
    }
}