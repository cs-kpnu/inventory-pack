using InventoryPack.Data;
using InventoryPack.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Features.RejectedRows;

internal static class RejectedRowMapping
{
    public static async Task<IReadOnlyList<RejectedRowDto>> MapAsync(
        AppDbContext db, IReadOnlyList<RejectedRow> rows, CancellationToken ct)
    {
        if (rows.Count == 0)
            return [];

        var rowIds = rows.Select(row => row.Id).ToList();
        var links = await db.RejectedRowIssues.AsNoTracking()
            .Where(link => rowIds.Contains(link.RejectedRowId))
            .Select(link => new
            {
                link.RejectedRowId, link.RejectionIssueId, link.RejectionIssue.Reason, link.RejectionIssue.Code
            })
            .ToListAsync(ct);

        var issueIds = links.Select(link => link.RejectionIssueId).Distinct().ToList();

        var participants = await db.RejectedRowIssues.AsNoTracking()
            .Where(link => issueIds.Contains(link.RejectionIssueId))
            .Select(link => new { link.RejectionIssueId, link.RejectedRowId, link.RejectedRow.RowNumber })
            .ToListAsync(ct);

        var rowsByIssue = participants.ToLookup(link => link.RejectionIssueId);
        var issuesByRow = links.ToLookup(link => link.RejectedRowId);

        return rows.Select(row =>
        {
            var issues = issuesByRow[row.Id]
                .OrderBy(link => link.Reason.ToString(), StringComparer.Ordinal)
                .ThenBy(link => link.Code, StringComparer.Ordinal)
                .ThenBy(link => link.RejectionIssueId)
                .Select(link => new RejectionIssueDto(
                    link.RejectionIssueId,
                    link.Reason.ToString(),
                    link.Code,
                    rowsByIssue[link.RejectionIssueId]
                        .Where(participant => participant.RejectedRowId != row.Id)
                        .Select(participant => participant.RowNumber)
                        .Distinct().Order().ToList()))
                .ToList();

            return new RejectedRowDto(
                row.Id, row.RowNumber, row.RawText, row.Quantity, row.Unit, row.Mvo, row.Subaccount,
                row.ParsedName, row.ParsedCodeCount, issues);
        }).ToList();
    }
}
