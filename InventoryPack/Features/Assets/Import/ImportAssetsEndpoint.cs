namespace InventoryPack.Features.Assets.Import;

public static class ImportAssetsEndpoint
{
    public static void MapAssetImportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/assets/import", async (
                IFormFile file,
                ImportAssetsHandler handler,
                CancellationToken ct) =>
            {
                if (file is not { Length: > 0 })
                    return Results.BadRequest("File is missing or empty.");

                if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    return Results.BadRequest("Only .xlsx spreadsheets are currently supported.");

                try
                {
                    await using var stream = file.OpenReadStream();
                    var response = await handler.HandleAsync(stream, ct);
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest($"Failed to parse spreadsheet: {ex.Message}");
                }
            })
            .WithSummary("Import assets from Excel spreadsheet (.xlsx)")
            .DisableAntiforgery();
    }
}