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
            var issues = GetLocalIssues(row);
            if (issues.Count > 0)
            {
                var rejectedRow = CreateRejected(row);
                foreach (var issue in issues)
                    Link(rejectedRow, issue);
                rejected.Add(rejectedRow);
            }
            else
            {
                candidates.Add(row);
            }
        }

        var conflictingGroups = candidates
            .SelectMany((row, index) => row.InventoryNumbers.Select(code => (Code: code, Row: row, Index: index)))
            .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Select(x => NormalizeName(x.Row.Name)).Distinct(StringComparer.Ordinal).Count() > 1)
            .ToList();

        var rejectedCandidates = new Dictionary<int, RejectedRow>();
        foreach (var group in conflictingGroups)
        {
            var issue = CreateIssue(RejectionReason.ConflictingNamesForCode, group.Key);
            foreach (var participant in group)
            {
                if (!rejectedCandidates.TryGetValue(participant.Index, out var rejectedRow))
                {
                    rejectedRow = CreateRejected(participant.Row);
                    rejectedCandidates.Add(participant.Index, rejectedRow);
                }

                Link(rejectedRow, issue);
            }
        }

        var validatedRows = new List<ValidatedAssetRow>();

        for (var index = 0; index < candidates.Count; index++)
            if (rejectedCandidates.TryGetValue(index, out var rejectedRow))
                rejected.Add(rejectedRow);
            else
                validatedRows.Add(CreateValidatedRow(candidates[index]));

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

    private static RejectedRow CreateRejected(ParsedAssetRow row)
    {
        return new RejectedRow
        {
            RowNumber = row.RowNumber,
            RawText = row.RawText,
            ParsedName = row.Name,
            ParsedCodeCount = row.InventoryNumbers.Count,
            Unit = row.RawUnit,
            Mvo = row.Mvo,
            Subaccount = row.Subaccount,
            Quantity = row.Quantity
        };
    }

    private static void Link(RejectedRow row, RejectionIssue issue)
    {
        var link = new RejectedRowIssue
        {
            RejectedRowId = row.Id, RejectedRow = row, RejectionIssueId = issue.Id, RejectionIssue = issue
        };
        row.Issues.Add(link);
        issue.Rows.Add(link);
    }

    private static RejectionIssue CreateIssue(RejectionReason reason, string? code = null)
    {
        var requiresCode = reason is RejectionReason.RepeatedCodeWithinRow or RejectionReason.ConflictingNamesForCode;
        if (requiresCode ? string.IsNullOrWhiteSpace(code) : code is not null)
            throw new ArgumentException("Only code-related rejection issues require an offending code.", nameof(code));

        return new RejectionIssue { Reason = reason, Code = code };
    }

    private static List<RejectionIssue> GetLocalIssues(ParsedAssetRow row)
    {
        var issues = new List<RejectionIssue>();

        if (string.IsNullOrWhiteSpace(row.Subaccount) || string.IsNullOrWhiteSpace(row.Name))
            issues.Add(CreateIssue(RejectionReason.MissingContext));

        if (row.Quantity is null or <= 0)
            issues.Add(CreateIssue(RejectionReason.InvalidQuantity));

        if (row.InventoryNumbers.Count == 0)
            issues.Add(CreateIssue(RejectionReason.NoValidCodes));

        foreach (var repeatedCode in row.InventoryNumbers
                     .GroupBy(code => code, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1))
            issues.Add(CreateIssue(RejectionReason.RepeatedCodeWithinRow, repeatedCode.Key));

        if (row.Quantity is > 0 && row.Quantity % 1 == 0 && row.InventoryNumbers.Count > 1 &&
            row.InventoryNumbers.Count != row.Quantity.Value)
            issues.Add(CreateIssue(RejectionReason.CodeQuantityMismatch));

        return issues;
    }
}
