using InventoryPack.Data;
using InventoryPack.Features.Assets;
using InventoryPack.Features.Printing;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Features.LedgerEntries;

public class LedgerEntryQueryHandler(AppDbContext db)
{
    public async Task<LedgerEntryListResponse> GetPagedAsync(
        string? search,
        int page = 1,
        int pageSize = 25,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, 100);

        var query = db.LedgerEntries.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                SqliteCustomFunctions.ContainsIgnoreCase(e.Name, term) ||
                (e.Mvo != null && SqliteCustomFunctions.ContainsIgnoreCase(e.Mvo, term)) ||
                SqliteCustomFunctions.ContainsIgnoreCase(e.Subaccount, term) ||
                e.Codes.Any(c => SqliteCustomFunctions.ContainsIgnoreCase(c.CodeGroup.Code, term)));
        }

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
            .Select(e => new
            {
                e.Id,
                e.SourceRowNumber,
                e.Name,
                e.Quantity,
                e.Unit,
                e.Mvo,
                e.Subaccount
            })
            .ToListAsync(ct);

        var rowIds = rows.Select(e => e.Id).ToList();
        var codes = await db.LedgerEntryCodes.AsNoTracking()
            .Where(c => rowIds.Contains(c.LedgerEntryId))
            .Select(c => new
            {
                c.LedgerEntryId,
                c.CodeGroupId,
                c.CodeGroup.Code,
                RegisteredAssetCount = c.CodeGroup.Assets.Count(),
                SourceRowCount = c.CodeGroup.LedgerCodes.Count()
            })
            .ToListAsync(ct);

        var codesByRow = codes
            .GroupBy(c => c.LedgerEntryId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(c => c.Code)
                    .ThenBy(c => c.CodeGroupId)
                    .Select(c => new LedgerEntryCodeDto(
                        c.CodeGroupId,
                        c.Code,
                        c.RegisteredAssetCount,
                        c.SourceRowCount))
                    .ToList());

        var items = rows.Select(e => new LedgerEntrySummaryDto(
            e.Id,
            e.SourceRowNumber,
            e.Name,
            e.Quantity,
            e.Unit,
            e.Mvo,
            e.Subaccount,
            codesByRow.GetValueOrDefault(e.Id) ?? []
        )).ToList();

        return new LedgerEntryListResponse(items, page, pageSize, totalCount, hasRejectedRows);
    }

    public async Task<LedgerEntryDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var row = await db.LedgerEntries.AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.SourceRowNumber,
                e.SourceTitle,
                e.Name,
                e.Quantity,
                e.Unit,
                e.Mvo,
                e.Subaccount
            })
            .FirstOrDefaultAsync(ct);

        if (row is null) return null;

        var codes = await db.LedgerEntryCodes.AsNoTracking()
            .Where(c => c.LedgerEntryId == id)
            .Select(c => new
            {
                c.CodeGroupId,
                c.CodeGroup.Code,
                SourceRowCount = c.CodeGroup.LedgerCodes.Count()
            })
            .ToListAsync(ct);

        var codeGroupIds = codes.Select(c => c.CodeGroupId).ToList();
        var assets = await db.Assets.AsNoTracking()
            .Where(a => codeGroupIds.Contains(a.CodeGroupId))
            .OrderBy(a => a.AllocatedAt)
            .ThenBy(a => a.Id)
            .Select(a => new { a.CodeGroupId, a.Id, a.AllocatedAt })
            .ToListAsync(ct);

        var assetsByCode = assets
            .GroupBy(a => a.CodeGroupId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(a => new RegisteredAssetDto(
                    a.Id,
                    TagPayloadFormatter.Format(a.Id),
                    DateTime.SpecifyKind(a.AllocatedAt, DateTimeKind.Utc)
                )).ToList());

        var detailCodes = codes
            .OrderBy(c => c.Code)
            .ThenBy(c => c.CodeGroupId)
            .Select(c => new LedgerEntryDetailCodeDto(
                c.CodeGroupId,
                c.Code,
                c.SourceRowCount,
                assetsByCode.GetValueOrDefault(c.CodeGroupId) ?? []
            ))
            .ToList();

        return new LedgerEntryDetailDto(
            row.Id,
            row.SourceRowNumber,
            row.SourceTitle,
            row.Name,
            row.Quantity,
            row.Unit,
            row.Mvo,
            row.Subaccount,
            detailCodes
        );
    }
}