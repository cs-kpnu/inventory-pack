namespace InventoryPack.Features.Assets.Import;

public static partial class ImportAssetsEndpoint
{
    public static void MapAssetImportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/assets/import", async (
                IFormFile file,
                ImportAssetsHandler handler,
                ILoggerFactory loggerFactory,
                CancellationToken ct) =>
            {
                var logger = loggerFactory.CreateLogger(typeof(ImportAssetsEndpoint));

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
                catch (ImportAlreadyExistsException ex)
                {
                    return Results.Problem(statusCode: StatusCodes.Status409Conflict, detail: ex.Message);
                }
                catch (NotSupportedException ex)
                {
                    return Results.Problem(statusCode: StatusCodes.Status415UnsupportedMediaType, detail: ex.Message);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    LogImportFailure(logger, ex, file.FileName);
                    return Results.Problem(statusCode: StatusCodes.Status500InternalServerError,
                        title: "An unexpected error occurred while processing the import file.");
                }
            })
            .WithSummary("Import assets from spreadsheet or ledger file")
            .DisableAntiforgery();
    }

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Unexpected error occurred while processing import file '{FileName}'")]
    private static partial void LogImportFailure(ILogger logger, Exception exception, string fileName);
}
