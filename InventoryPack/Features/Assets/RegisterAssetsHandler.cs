using InventoryPack.Data;
using InventoryPack.Data.Entities;
using InventoryPack.Features.Printing;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Features.Assets;

public abstract record RegisterAssetsResult
{
    public sealed record Success(IReadOnlyList<RegisteredAssetDto> Assets) : RegisterAssetsResult;

    public sealed record IdempotentReplay(IReadOnlyList<RegisteredAssetDto> Assets) : RegisterAssetsResult;

    public sealed record NotFound(string Message) : RegisterAssetsResult;

    public sealed record Conflict(string Message) : RegisterAssetsResult;

    public sealed record BadRequest(string Message) : RegisterAssetsResult;
}

public class RegisterAssetsHandler(AppDbContext db)
{
    public async Task<RegisterAssetsResult> HandleAsync(
        RegisterAssetsRequest request,
        CancellationToken ct = default)
    {
        if (request.Count is < 1 or > 50)
            return new RegisterAssetsResult.BadRequest("Count must be between 1 and 50.");

        if (request.RequestId == Guid.Empty)
            return new RegisterAssetsResult.BadRequest("RequestId cannot be empty.");

        if (request.LedgerEntryId == Guid.Empty)
            return new RegisterAssetsResult.BadRequest("LedgerEntryId cannot be empty.");

        if (request.CodeGroupId == Guid.Empty)
            return new RegisterAssetsResult.BadRequest("CodeGroupId cannot be empty.");

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var existing = await db.Assets
            .Where(a => a.RequestId == request.RequestId)
            .ToListAsync(ct);

        if (existing.Count > 0)
        {
            await tx.RollbackAsync(ct);

            if (existing.Count == request.Count &&
                existing.All(a => a.CodeGroupId == request.CodeGroupId &&
                                  a.RegisteredFromLedgerEntryId == request.LedgerEntryId))
                return new RegisterAssetsResult.IdempotentReplay(MapToDtos(existing));

            return new RegisterAssetsResult.Conflict(
                $"RequestId '{request.RequestId}' has already been used with different parameters.");
        }

        var ledgerEntry = await db.LedgerEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.LedgerEntryId, ct);

        if (ledgerEntry is null)
        {
            await tx.RollbackAsync(ct);
            return new RegisterAssetsResult.NotFound($"LedgerEntry with ID '{request.LedgerEntryId}' was not found.");
        }

        var linkExists = await db.LedgerEntryCodes
            .AnyAsync(lec => lec.LedgerEntryId == request.LedgerEntryId && lec.CodeGroupId == request.CodeGroupId, ct);

        if (!linkExists)
        {
            await tx.RollbackAsync(ct);

            var groupExists = await db.CodeGroups.AnyAsync(g => g.Id == request.CodeGroupId, ct);
            if (!groupExists)
                return new RegisterAssetsResult.NotFound($"CodeGroup with ID '{request.CodeGroupId}' was not found.");

            return new RegisterAssetsResult.BadRequest(
                $"CodeGroup '{request.CodeGroupId}' is not associated with LedgerEntry '{request.LedgerEntryId}'.");
        }

        var normalizedMvo = MvoNormalizer.Normalize(ledgerEntry.Mvo);

        var now = DateTime.UtcNow;
        var newAssets = new List<Asset>(request.Count);
        for (var i = 0; i < request.Count; i++)
            newAssets.Add(new Asset
            {
                Id = Guid.NewGuid(),
                CodeGroupId = request.CodeGroupId,
                RequestId = request.RequestId,
                RegisteredForMvo = normalizedMvo,
                RegisteredFromLedgerEntryId = request.LedgerEntryId,
                AllocatedAt = now
            });

        db.Assets.AddRange(newAssets);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new RegisterAssetsResult.Success(MapToDtos(newAssets));
    }

    private static List<RegisteredAssetDto> MapToDtos(IEnumerable<Asset> assets)
    {
        return assets
            .OrderBy(a => a.AllocatedAt)
            .ThenBy(a => a.Id)
            .Select(a => new RegisteredAssetDto(
                a.Id,
                TagPayloadFormatter.Format(a.Id),
                DateTime.SpecifyKind(a.AllocatedAt, DateTimeKind.Utc),
                a.RegisteredForMvo
            ))
            .ToList();
    }
}
