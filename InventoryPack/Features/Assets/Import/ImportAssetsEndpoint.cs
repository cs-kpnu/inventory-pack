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

                var extension = Path.GetExtension(file.FileName.Trim('"', ' ')).ToLowerInvariant();
                if (string.IsNullOrEmpty(extension))
                    return Results.BadRequest("Uploaded file does not have a file extension.");

                try
                {
                    await using var stream = file.OpenReadStream();
                    var response = await handler.HandleAsync(extension, stream, ct);
                    return Results.Ok(response);
                }
                catch (NotSupportedException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest($"Failed to parse file: {ex.Message}");
                }
            })
            .WithSummary("Import assets from spreadsheet or ledger file")
            .DisableAntiforgery();
    }
}