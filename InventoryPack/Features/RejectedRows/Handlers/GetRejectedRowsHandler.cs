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
            .Select(r => new
            {
                r.Id,
                r.RowNumber,
                r.RawText,
                r.Quantity,
                r.Unit,
                r.Mvo,
                r.Subaccount
            })
            .ToListAsync(ct);

        var rowIds = rows.Select(r => r.Id).ToList();
        var reasonsByRow = await db.RejectedRowReasons.AsNoTracking()
            .Where(re => rowIds.Contains(re.RejectedRowId))
            .ToListAsync(ct);

        var reasonsLookup = reasonsByRow
            .GroupBy(re => re.RejectedRowId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g.Select(x => x.Reason.ToString()).OrderBy(s => s).ToList());

        var items = rows.Select(r => new RejectedRowDto(
            r.Id,
            r.RowNumber,
            r.RawText,
            r.Quantity,
            r.Unit,
            r.Mvo,
            r.Subaccount,
            reasonsLookup.GetValueOrDefault(r.Id, [])
        )).ToList();

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

        return query.Where(r => r.Reasons.Any(re => re.Reason == reason.Value));
    }

    private async Task<IReadOnlyDictionary<string, int>> LoadTotalReasonCountsAsync(CancellationToken ct)
    {
        var counts = await db.RejectedRowReasons.AsNoTracking()
            .GroupBy(re => re.Reason)
            .Select(g => new { Reason = g.Key.ToString(), Count = g.Count() })
            .ToListAsync(ct);

        return counts.ToDictionary(x => x.Reason, x => x.Count);
    }
}
