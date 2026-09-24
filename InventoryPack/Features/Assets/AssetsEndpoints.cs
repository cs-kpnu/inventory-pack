using InventoryPack.Features.Assets.Handlers;
using InventoryPack.Features.Assets.Import;

namespace InventoryPack.Features.Assets;

public static partial class AssetsEndpoints
{
    public static void MapAssetsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/assets", RegisterAssetsAsync)
            .WithTags("Assets")
            .WithSummary("Register physical asset identities under a code group");

        app.MapPost("/api/assets/import", ImportAssetsAsync)
            .WithTags("Assets")
            .WithSummary("Import assets from spreadsheet or ledger file")
            .DisableAntiforgery();
    }

    private static async Task<IResult> RegisterAssetsAsync(
        RegisterAssetsRequest request,
        RegisterAssetsHandler handler,
        CancellationToken ct)
    {
        var result = await handler.HandleAsync(request, ct);

        return result switch
        {
            RegisterAssetsResult.Success success => Results.Json(success.Assets,
                statusCode: StatusCodes.Status201Created),
            RegisterAssetsResult.IdempotentReplay replay => Results.Ok(replay.Assets),
            RegisterAssetsResult.NotFound notFound => Results.NotFound(new { error = notFound.Message }),
            RegisterAssetsResult.Conflict conflict => Results.Conflict(new { error = conflict.Message }),
            RegisterAssetsResult.BadRequest badRequest => Results.BadRequest(new { error = badRequest.Message }),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    private static async Task<IResult> ImportAssetsAsync(
        IFormFile file,
        ImportAssetsHandler handler,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        var logger = loggerFactory.CreateLogger(typeof(AssetsEndpoints));
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
    }

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Unexpected error occurred while processing import file '{FileName}'")]
    private static partial void LogImportFailure(ILogger logger, Exception exception, string fileName);
}
