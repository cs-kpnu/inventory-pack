using System.Text.RegularExpressions;
using InventoryPack.Data.Entities;

namespace InventoryPack.Features.Assets.Import;

public static partial class AssetImportValidator
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    public static ImportResult Validate(IEnumerable<ParsedAssetRow> rows)
    {
        var candidates = new List<ParsedAssetRow>();
        var rejected = new List<RejectedRow>();

        foreach (var row in rows)
        {
            var reasons = GetRejectionReasons(row);
            if (reasons.Count > 0)
                rejected.Add(CreateRejected(row, reasons));
            else
                candidates.Add(row);
        }

        var conflictingCodes = candidates
            .SelectMany(r => r.InventoryNumbers.Select(c => (Code: c, Row: r)))
            .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Select(x => NormalizeName(x.Row.Name)).Distinct(StringComparer.Ordinal).Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var validatedRows = new List<ValidatedAssetRow>();

        foreach (var row in candidates)
            if (row.InventoryNumbers.Any(conflictingCodes.Contains))
                rejected.Add(CreateRejected(row, [RejectionReason.DuplicateCode]));
            else
                validatedRows.Add(CreateValidatedRow(row));

        return new ImportResult(validatedRows, rejected);
    }

    private static ValidatedAssetRow CreateValidatedRow(ParsedAssetRow row)
    {
        return new ValidatedAssetRow(
            row.RowNumber,
            row.RawText,
            row.RawUnit,
            row.Name ?? string.Empty,
            row.Mvo,
            row.Subaccount ?? string.Empty,
            row.Quantity!.Value,
            row.InventoryNumbers
        );
    }

    private static string NormalizeName(string? name)
    {
        return string.IsNullOrWhiteSpace(name)
            ? string.Empty
            : WhitespaceRegex().Replace(name.Trim().ToLowerInvariant().TrimEnd('.', ',', ';', ':', '-', ' '), " ");
    }

    private static RejectedRow CreateRejected(ParsedAssetRow row, IEnumerable<RejectionReason> reasons)
    {
        var rejected = new RejectedRow
        {
            RowNumber = row.RowNumber,
            RawText = row.RawText,
            Unit = row.RawUnit,
            Mvo = row.Mvo,
            Subaccount = row.Subaccount,
            Quantity = row.Quantity
        };

        foreach (var reason in reasons.Distinct())
            rejected.Reasons.Add(new RejectedRowReason { Reason = reason, RejectedRow = rejected });

        return rejected;
    }

    private static List<RejectionReason> GetRejectionReasons(ParsedAssetRow row)
    {
        var reasons = new List<RejectionReason>();

        if (string.IsNullOrWhiteSpace(row.Subaccount) || string.IsNullOrWhiteSpace(row.Name))
            reasons.Add(RejectionReason.MissingContext);

        if (row.Quantity is null or <= 0)
            reasons.Add(RejectionReason.InvalidQuantity);

        if (row.InventoryNumbers.Count == 0)
            reasons.Add(RejectionReason.NoSingleCode);

        if (row.InventoryNumbers.Count > 1 &&
            row.InventoryNumbers.Distinct(StringComparer.OrdinalIgnoreCase).Count() != row.InventoryNumbers.Count)
            reasons.Add(RejectionReason.DuplicateCode);

        if (row.Quantity is > 0 && row.Quantity % 1 == 0 && row.InventoryNumbers.Count > 1 &&
            row.InventoryNumbers.Count != row.Quantity.Value)
            reasons.Add(RejectionReason.CodeQuantityMismatch);

        return reasons;
    }
}
