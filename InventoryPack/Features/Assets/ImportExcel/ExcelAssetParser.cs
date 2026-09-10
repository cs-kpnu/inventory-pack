using System.Text.RegularExpressions;
using InventoryPack.Data.Entities;
using MiniExcelLibs;

namespace InventoryPack.Features.Assets.ImportExcel;

public static partial class ExcelAssetParser
{
    [GeneratedRegex(@"^(.*?)\s*\(\s*(?=.*\d)(\w{6,})\s*\)\s*$")]
    private static partial Regex SingleCodeRegex();

    public static ImportResult Parse(Stream stream)
    {
        var assets = new List<Asset>();
        var rejected = new List<RejectedRow>();
        var rowNumber = 0;

        foreach (var row in stream.Query<ExcelAssetRow>(hasHeader: false))
        {
            rowNumber++;

            // Skip spreadsheet noise (empty rows, page breaks, repeated column headers)
            if (string.IsNullOrWhiteSpace(row.Title) || row.Title.StartsWith('-') || row.Title.Contains("Найменування"))
                continue;

            // Skip rows with no quantity
            if (row.Quantity is null)
                continue;

            // Best Case: 1 unit, valid MVO/Subaccount, single code in parentheses
            var match = SingleCodeRegex().Match(row.Title);

            if (row.Quantity == 1 && match.Success && !string.IsNullOrEmpty(row.Mvo) &&
                !string.IsNullOrEmpty(row.Subaccount))
                assets.Add(new Asset
                {
                    InventoryNumber = match.Groups[2].Value,
                    Name = match.Groups[1].Value.Trim(),
                    Mvo = row.Mvo.Trim(),
                    Subaccount = row.Subaccount.Trim()
                });
            else
                rejected.Add(new RejectedRow
                {
                    RowNumber = rowNumber,
                    RawText = row.Title,
                    Reason = row.Quantity != 1 ? RejectionReason.MultipleQuantity : RejectionReason.NoSingleCode,
                    Mvo = row.Mvo?.Trim(),
                    Subaccount = row.Subaccount?.Trim()
                });
        }

        return new ImportResult(assets, rejected);
    }
}