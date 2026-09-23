using InventoryPack.Data;
using InventoryPack.Data.Entities;
using InventoryPack.Features.Assets;
using InventoryPack.Features.Printing;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Features.LedgerEntries;

public class LedgerEntryQueryHandler(AppDbContext db)
{
    public async Task<LedgerEntryListResponse> GetPagedAsync(
        string? search,
        string? status = null,
        int page = 1,
        int pageSize = 25,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, 100);

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

    public async Task<LedgerEntryDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var row = await db.LedgerEntries.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (row is null)
            return null;

        var codes = await db.LedgerEntryCodes.AsNoTracking()
            .Where(c => c.LedgerEntryId == id)
            .Select(c => new CodeHeader(c.CodeGroupId, c.CodeGroup.Code, c.CodeGroup.LedgerCodes.Count()))
            .ToListAsync(ct);

        var assetsByCode = await LoadAssetsAsync(codes.Select(c => c.Id).ToList(), ct);
        var detailCodes = new List<LedgerEntryDetailCodeDto>(codes.Count);
        foreach (var code in codes.OrderBy(c => c.Code).ThenBy(c => c.Id))
            detailCodes.Add(CreateDetailCode(code, assetsByCode[code.Id].ToList(), row.Mvo));

        return new LedgerEntryDetailDto(
            row.Id, row.SourceRowNumber, row.SourceTitle, row.Name,
            row.Quantity, row.Unit, row.Mvo, row.Subaccount, detailCodes);
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

    private static IQueryable<LedgerEntry> FilterByStatus(IQueryable<LedgerEntry> query, string? status)
    {
        return status switch
        {
            "registered" => query.Where(e =>
                e.Mvo != null && e.Mvo.Trim() != "" &&
                e.Codes.Any() &&
                e.Codes.All(c => c.CodeGroup.Assets.Any(a =>
                    SqliteCustomFunctions.EqualsIgnoreCase(a.RegisteredForMvo, e.Mvo)))),

            "partial" => query.Where(e =>
                e.Mvo != null && e.Mvo.Trim() != "" &&
                e.Codes.Any(c => c.CodeGroup.Assets.Any(a =>
                    SqliteCustomFunctions.EqualsIgnoreCase(a.RegisteredForMvo, e.Mvo))) &&
                e.Codes.Any(c => !c.CodeGroup.Assets.Any(a =>
                    SqliteCustomFunctions.EqualsIgnoreCase(a.RegisteredForMvo, e.Mvo)))),

            "unregistered" => query.Where(e =>
                e.Mvo == null || e.Mvo.Trim() == "" ||
                !e.Codes.Any(c => c.CodeGroup.Assets.Any(a =>
                    SqliteCustomFunctions.EqualsIgnoreCase(a.RegisteredForMvo, e.Mvo)))),

            _ => query
        };
    }

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

    private async Task<ILookup<Guid, RegisteredAssetDto>> LoadAssetsAsync(
        List<Guid> codeGroupIds, CancellationToken ct)
    {
        var assets = await db.Assets.AsNoTracking()
            .Where(a => codeGroupIds.Contains(a.CodeGroupId))
            .OrderBy(a => a.AllocatedAt)
            .ThenBy(a => a.Id)
            .Select(a => new { a.CodeGroupId, a.Id, a.AllocatedAt, a.RegisteredForMvo })
            .ToListAsync(ct);

        return assets.ToLookup(a => a.CodeGroupId, a => new RegisteredAssetDto(
            a.Id,
            TagPayloadFormatter.Format(a.Id),
            DateTime.SpecifyKind(a.AllocatedAt, DateTimeKind.Utc),
            a.RegisteredForMvo));
    }

    private static LedgerEntryDetailCodeDto CreateDetailCode(
        CodeHeader code, List<RegisteredAssetDto> assets, string? mvo)
    {
        var normalizedMvo = MvoNormalizer.Normalize(mvo);
        var mvoCount = normalizedMvo is null
            ? 0
            : assets.Count(a => MvoNormalizer.Equals(a.RegisteredForMvo, normalizedMvo));
        var unassignedCount = assets.Count(a => a.RegisteredForMvo == null);

        return new LedgerEntryDetailCodeDto(
            code.Id, code.Code, assets.Count, mvoCount, unassignedCount, code.SourceRowCount, assets);
    }

    private sealed record CodeHeader(Guid Id, string Code, int SourceRowCount);
}
