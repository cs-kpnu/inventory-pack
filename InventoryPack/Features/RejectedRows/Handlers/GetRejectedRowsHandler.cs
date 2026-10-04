using InventoryPack.Data;
using InventoryPack.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Features.RejectedRows.Handlers;

public class GetRejectedRowsHandler(AppDbContext db)
{
    public async Task<RejectedRowListResponse> HandleAsync(
        string? search,
        RejectionReason? reason = null,
        int page = 1,
        int pageSize = 25,
        CancellationToken ct = default)
    {
        var query = FilterBySearch(db.RejectedRows.AsNoTracking(), search);
        query = FilterByReason(query, reason);

        var totalCount = await query.CountAsync(ct);
        var totalReasonCounts = await LoadTotalReasonCountsAsync(ct);

        var offset = (long)(page - 1) * pageSize;
        if (offset >= totalCount)
            return new RejectedRowListResponse([], page, pageSize, totalCount, totalReasonCounts);

        var rows = await query
            .OrderBy(r => r.RowNumber)
            .ThenBy(r => r.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = await RejectedRowMapping.MapAsync(db, rows, ct);

        return new RejectedRowListResponse(items, page, pageSize, totalCount, totalReasonCounts);
    }

    private static IQueryable<RejectedRow> FilterBySearch(IQueryable<RejectedRow> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var term = search.Trim();

        if (int.TryParse(term, out var rowNum))
            return query.Where(r =>
                r.RowNumber == rowNum ||
                SqliteCustomFunctions.ContainsIgnoreCase(r.RawText, term) ||
                (r.Mvo != null && SqliteCustomFunctions.ContainsIgnoreCase(r.Mvo, term)) ||
                (r.Subaccount != null && SqliteCustomFunctions.ContainsIgnoreCase(r.Subaccount, term)) ||
                (r.Unit != null && SqliteCustomFunctions.ContainsIgnoreCase(r.Unit, term)));

        return query.Where(r =>
            SqliteCustomFunctions.ContainsIgnoreCase(r.RawText, term) ||
            (r.Mvo != null && SqliteCustomFunctions.ContainsIgnoreCase(r.Mvo, term)) ||
            (r.Subaccount != null && SqliteCustomFunctions.ContainsIgnoreCase(r.Subaccount, term)) ||
            (r.Unit != null && SqliteCustomFunctions.ContainsIgnoreCase(r.Unit, term)));
    }

    private static IQueryable<RejectedRow> FilterByReason(IQueryable<RejectedRow> query, RejectionReason? reason)
    {
        if (reason is null)
            return query;

        return query.Where(r => r.Issues.Any(link => link.RejectionIssue.Reason == reason.Value));
    }

    private async Task<IReadOnlyDictionary<string, int>> LoadTotalReasonCountsAsync(CancellationToken ct)
    {
        var counts = await db.RejectedRowIssues.AsNoTracking()
            .GroupBy(link => link.RejectionIssue.Reason)
            .Select(g => new
            {
                Reason = g.Key.ToString(), Count = g.Select(link => link.RejectedRowId).Distinct().Count()
            })
            .ToListAsync(ct);

        return counts.ToDictionary(x => x.Reason, x => x.Count);
    }
}
