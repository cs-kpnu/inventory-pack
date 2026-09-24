using InventoryPack.Data;
using InventoryPack.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Features.LedgerEntries.Handlers;

public class GetLedgerEntriesHandler(AppDbContext db)
{
    public async Task<LedgerEntryListResponse> HandleAsync(
        string? search,
        LedgerEntryStatus? status = null,
        int page = 1,
        int pageSize = 25,
        CancellationToken ct = default)
    {
        var query = FilterBySearch(db.LedgerEntries.AsNoTracking(), search);
        query = FilterByStatus(query, status);

        var totalCount = await query.CountAsync(ct);
        var hasRejectedRows = await db.RejectedRows.AnyAsync(ct);
        var offset = (long)(page - 1) * pageSize;
        if (offset >= totalCount)
            return new LedgerEntryListResponse([], page, pageSize, totalCount, hasRejectedRows);

        var rows = await query
            .OrderBy(e => e.SourceRowNumber)
            .ThenBy(e => e.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Select(e => new LedgerEntrySummaryDto(
                e.Id, e.SourceRowNumber, e.Name, e.Quantity, e.Unit, e.Mvo, e.Subaccount,
                new List<LedgerEntryCodeDto>()))
            .ToListAsync(ct);

        var codesByRow = await LoadCodeSummariesAsync(rows.Select(r => r.Id).ToList(), ct);
        var items = rows.Select(row => row with { Codes = codesByRow[row.Id].ToList() }).ToList();

        return new LedgerEntryListResponse(items, page, pageSize, totalCount, hasRejectedRows);
    }

    private static IQueryable<LedgerEntry> FilterBySearch(IQueryable<LedgerEntry> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var term = search.Trim();

        return query.Where(e =>
            SqliteCustomFunctions.ContainsIgnoreCase(e.Name, term) ||
            (e.Mvo != null && SqliteCustomFunctions.ContainsIgnoreCase(e.Mvo, term)) ||
            SqliteCustomFunctions.ContainsIgnoreCase(e.Subaccount, term) ||
            e.Codes.Any(c => SqliteCustomFunctions.ContainsIgnoreCase(c.CodeGroup.Code, term)));
    }

    private static IQueryable<LedgerEntry> FilterByStatus(IQueryable<LedgerEntry> query, LedgerEntryStatus? status) =>
        status switch
        {
            LedgerEntryStatus.Registered => FilterRegistered(query),
            LedgerEntryStatus.Partial => FilterPartial(query),
            LedgerEntryStatus.Unregistered => FilterUnregistered(query),
            null => query,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };

    private static IQueryable<LedgerEntry> FilterRegistered(IQueryable<LedgerEntry> query) =>
        query.Where(e =>
            e.Mvo != null && e.Mvo.Trim() != "" &&
            e.Codes.Any() &&
            e.Codes.All(c => c.CodeGroup.Assets.Any(a =>
                SqliteCustomFunctions.EqualsIgnoreCase(a.RegisteredForMvo, e.Mvo))));

    private static IQueryable<LedgerEntry> FilterPartial(IQueryable<LedgerEntry> query) =>
        query.Where(e =>
            e.Mvo != null && e.Mvo.Trim() != "" &&
            e.Codes.Any(c => c.CodeGroup.Assets.Any(a =>
                SqliteCustomFunctions.EqualsIgnoreCase(a.RegisteredForMvo, e.Mvo))) &&
            e.Codes.Any(c => !c.CodeGroup.Assets.Any(a =>
                SqliteCustomFunctions.EqualsIgnoreCase(a.RegisteredForMvo, e.Mvo))));

    private static IQueryable<LedgerEntry> FilterUnregistered(IQueryable<LedgerEntry> query) =>
        query.Where(e =>
            e.Mvo == null || e.Mvo.Trim() == "" ||
            !e.Codes.Any(c => c.CodeGroup.Assets.Any(a =>
                SqliteCustomFunctions.EqualsIgnoreCase(a.RegisteredForMvo, e.Mvo))));

    private async Task<ILookup<Guid, LedgerEntryCodeDto>> LoadCodeSummariesAsync(
        List<Guid> rowIds, CancellationToken ct)
    {
        var codes = await db.LedgerEntryCodes.AsNoTracking()
            .Where(c => rowIds.Contains(c.LedgerEntryId))
            .Select(c => new
            {
                c.LedgerEntryId,
                Id = c.CodeGroupId,
                c.CodeGroup.Code,
                GroupCount = c.CodeGroup.Assets.Count(),
                MvoCount = c.CodeGroup.Assets.Count(a =>
                    SqliteCustomFunctions.EqualsIgnoreCase(a.RegisteredForMvo, c.LedgerEntry.Mvo)),
                UnassignedCount = c.CodeGroup.Assets.Count(a => a.RegisteredForMvo == null),
                SourceRowCount = c.CodeGroup.LedgerCodes.Count()
            })
            .ToListAsync(ct);

        return codes
            .OrderBy(c => c.Code)
            .ThenBy(c => c.Id)
            .ToLookup(c => c.LedgerEntryId, c => new LedgerEntryCodeDto(
                c.Id, c.Code, c.GroupCount, c.MvoCount, c.UnassignedCount, c.SourceRowCount));
    }
}
