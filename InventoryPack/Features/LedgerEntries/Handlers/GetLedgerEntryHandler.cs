using InventoryPack.Data;
using InventoryPack.Features.Assets;
using InventoryPack.Features.Printing;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Features.LedgerEntries.Handlers;

public class GetLedgerEntryHandler(AppDbContext db)
{
    public async Task<LedgerEntryDetailDto?> HandleAsync(Guid id, CancellationToken ct = default)
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
