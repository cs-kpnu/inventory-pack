using InventoryPack.Data;
using InventoryPack.Features.Printing;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Features.CodeGroups;

public class CodeGroupQueryHandler(AppDbContext db)
{
    public async Task<CodeGroupListResponse> GetPagedAsync(
        string? search,
        string? status = null,
        int page = 1,
        int pageSize = 25,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, 100);

        var (groups, totalCount, hasRejectedRows) = await LoadGroupPageAsync(search, status, page, pageSize, ct);
        if (groups.Count == 0) return new CodeGroupListResponse([], page, pageSize, totalCount, hasRejectedRows);

        var groupIds = groups.Select(g => g.Id).ToList();
        var previews = await LoadRowPreviewsAsync(groupIds, ct);

        var items = groups.Select(g => new CodeGroupSummaryDto(
            g.Id,
            g.Code,
            g.RegisteredAssetCount,
            g.SourceRowCount,
            previews.GetValueOrDefault(g.Id) ?? []
        )).ToList();

        return new CodeGroupListResponse(items, page, pageSize, totalCount, hasRejectedRows);
    }

    public async Task<CodeGroupDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var header = await LoadGroupHeaderAsync(id, ct);
        if (header is null) return null;

        var sourceRows = await LoadSourceRowsAsync(id, ct);
        var assets = await LoadAssetsAsync(id, ct);

        return new CodeGroupDetailDto(
            header.Id,
            header.Code,
            header.RegisteredAssetCount,
            sourceRows,
            assets
        );
    }

    private async Task<(List<GroupPageItem> Groups, int TotalCount, bool HasRejectedRows)> LoadGroupPageAsync(
        string? search,
        string? status,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var query = db.CodeGroups.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(g =>
                g.Code.Contains(term) ||
                g.LedgerCodes.Any(lc =>
                    lc.LedgerEntry.Name.Contains(term) ||
                    (lc.LedgerEntry.Mvo != null && lc.LedgerEntry.Mvo.Contains(term)) ||
                    lc.LedgerEntry.Subaccount.Contains(term)));
        }

        if (string.Equals(status, "unregistered", StringComparison.OrdinalIgnoreCase))
            query = query.Where(g => !g.Assets.Any());
        else if (string.Equals(status, "registered", StringComparison.OrdinalIgnoreCase))
            query = query.Where(g => g.Assets.Any());

        var totalCount = await query.CountAsync(ct);
        var hasRejectedRows = await db.RejectedRows.AnyAsync(ct);

        var offset = (long)(page - 1) * pageSize;
        if (offset >= totalCount) return ([], totalCount, hasRejectedRows);

        var groups = await query
            .OrderBy(g => g.Code)
            .ThenBy(g => g.Id)
            .Skip((int)offset)
            .Take(pageSize)
            .Select(g => new GroupPageItem(
                g.Id,
                g.Code,
                g.Assets.Count(),
                g.LedgerCodes.Count()
            ))
            .ToListAsync(ct);

        return (groups, totalCount, hasRejectedRows);
    }

    private async Task<Dictionary<Guid, List<CodeGroupSourceRowDto>>> LoadRowPreviewsAsync(
        IReadOnlyList<Guid> groupIds,
        CancellationToken ct)
    {
        if (groupIds.Count == 0) return [];

        return await db.CodeGroups.AsNoTracking()
            .Where(g => groupIds.Contains(g.Id))
            .Select(g => new
            {
                g.Id,
                SampleRows = g.LedgerCodes
                    .OrderBy(lc => lc.LedgerEntry.SourceRowNumber)
                    .ThenBy(lc => lc.LedgerEntryId)
                    .Take(3)
                    .Select(lc => new CodeGroupSourceRowDto(
                        lc.LedgerEntry.Id,
                        lc.LedgerEntry.SourceRowNumber,
                        lc.LedgerEntry.Name,
                        lc.LedgerEntry.Quantity,
                        lc.LedgerEntry.Unit,
                        lc.LedgerEntry.Mvo,
                        lc.LedgerEntry.Subaccount
                    ))
                    .ToList()
            })
            .ToDictionaryAsync(x => x.Id, x => x.SampleRows, ct);
    }

    private async Task<GroupHeader?> LoadGroupHeaderAsync(Guid id, CancellationToken ct)
    {
        return await db.CodeGroups.AsNoTracking()
            .Where(g => g.Id == id)
            .Select(g => new GroupHeader(
                g.Id,
                g.Code,
                g.Assets.Count()
            ))
            .FirstOrDefaultAsync(ct);
    }

    private async Task<List<CodeGroupSourceRowDto>> LoadSourceRowsAsync(Guid codeGroupId, CancellationToken ct)
    {
        return await db.LedgerEntryCodes.AsNoTracking()
            .Where(lc => lc.CodeGroupId == codeGroupId)
            .OrderBy(lc => lc.LedgerEntry.SourceRowNumber)
            .ThenBy(lc => lc.LedgerEntryId)
            .Select(lc => new CodeGroupSourceRowDto(
                lc.LedgerEntry.Id,
                lc.LedgerEntry.SourceRowNumber,
                lc.LedgerEntry.Name,
                lc.LedgerEntry.Quantity,
                lc.LedgerEntry.Unit,
                lc.LedgerEntry.Mvo,
                lc.LedgerEntry.Subaccount
            ))
            .ToListAsync(ct);
    }

    private async Task<List<RegisteredAssetDto>> LoadAssetsAsync(Guid codeGroupId, CancellationToken ct)
    {
        var assets = await db.Assets.AsNoTracking()
            .Where(a => a.CodeGroupId == codeGroupId)
            .OrderBy(a => a.AllocatedAt)
            .ThenBy(a => a.Id)
            .Select(a => new
            {
                a.Id,
                a.AllocatedAt
            })
            .ToListAsync(ct);

        return assets
            .Select(a => new RegisteredAssetDto(
                a.Id,
                TagPayloadFormatter.Format(a.Id),
                a.AllocatedAt
            ))
            .ToList();
    }

    private sealed record GroupPageItem(
        Guid Id,
        string Code,
        int RegisteredAssetCount,
        int SourceRowCount
    );

    private sealed record GroupHeader(
        Guid Id,
        string Code,
        int RegisteredAssetCount
    );
}