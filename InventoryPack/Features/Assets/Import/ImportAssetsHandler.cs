using InventoryPack.Data;

namespace InventoryPack.Features.Assets.Import;

public class ImportAssetsHandler(IAssetImporter importer, AppDbContext db)
{
    public async Task<ImportResponse> HandleAsync(Stream stream, CancellationToken ct = default)
    {
        var result = importer.Import(stream);

        await db.Assets.AddRangeAsync(result.Assets, ct);
        await db.RejectedRows.AddRangeAsync(result.RejectedRows, ct);
        await db.SaveChangesAsync(ct);

        return new ImportResponse(result.Assets.Count, result.RejectedRows.Count);
    }
}

public record ImportResponse(int ImportedAssets, int RejectedRows);