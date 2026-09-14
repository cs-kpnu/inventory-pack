using InventoryPack.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Features.Assets.Import;

public class ImportAssetsHandler(IServiceProvider serviceProvider, AppDbContext db)
{
    public async Task<ImportResponse> HandleAsync(string fileExtension, Stream stream, CancellationToken ct = default)
    {
        var importer = serviceProvider.GetKeyedService<IAssetImporter>(fileExtension) ??
                       throw new NotSupportedException($"No asset importer for file extension '{fileExtension}'.");

        var parsedRows = importer.Parse(stream);
        var result = AssetImportValidator.Validate(parsedRows);

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        await db.RejectedRowReasons.ExecuteDeleteAsync(ct);
        await db.RejectedRows.ExecuteDeleteAsync(ct);

        await db.LedgerEntries.AddRangeAsync(result.LedgerEntries, ct);
        await db.RejectedRows.AddRangeAsync(result.RejectedRows, ct);
        await db.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return new ImportResponse(result.LedgerEntries.Count, result.TotalAssets, result.RejectedRows.Count);
    }
}

public record ImportResponse(int ImportedLedgerEntries, int ImportedAssets, int RejectedRows);