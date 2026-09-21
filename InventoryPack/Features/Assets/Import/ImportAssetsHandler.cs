using InventoryPack.Data;
using InventoryPack.Data.Entities;
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

        if (await db.LedgerEntries.AnyAsync(ct))
            throw new ImportAlreadyExistsException(
                "An import has already been completed. Repeated imports are not supported yet.");

        await db.RejectedRowReasons.ExecuteDeleteAsync(ct);
        await db.RejectedRows.ExecuteDeleteAsync(ct);

        var ledgerEntries = new List<LedgerEntry>(result.ValidatedRows.Count);
        foreach (var row in result.ValidatedRows)
        {
            var entry = new LedgerEntry
            {
                SourceRowNumber = row.SourceRowNumber,
                SourceTitle = row.SourceTitle,
                Unit = row.Unit,
                Name = row.Name,
                Mvo = row.Mvo,
                Subaccount = row.Subaccount,
                Quantity = row.Quantity
            };

            foreach (var code in row.InventoryNumbers)
                entry.Codes.Add(new LedgerEntryCode
                {
                    LedgerEntryId = entry.Id,
                    LedgerEntry = entry,
                    Code = code
                });

            ledgerEntries.Add(entry);
        }

        await db.LedgerEntries.AddRangeAsync(ledgerEntries, ct);
        await db.RejectedRows.AddRangeAsync(result.RejectedRows, ct);
        await db.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return new ImportResponse(ledgerEntries.Count, result.RejectedRows.Count);
    }
}

public record ImportResponse(int ImportedLedgerEntries, int RejectedRows);