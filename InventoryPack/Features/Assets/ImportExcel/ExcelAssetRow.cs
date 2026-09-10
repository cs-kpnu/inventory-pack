using MiniExcelLibs.Attributes;

namespace InventoryPack.Features.Assets.ImportExcel;

/// <summary>
///     Representation of an Excel row.
/// </summary>
public class ExcelAssetRow
{
    [ExcelColumnIndex("B")] public string? Mvo { get; set; }
    [ExcelColumnIndex("C")] public string? Subaccount { get; set; }
    [ExcelColumnIndex("D")] public string? Title { get; set; }
    [ExcelColumnIndex("W")] public decimal? Quantity { get; set; }
}