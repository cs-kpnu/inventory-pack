namespace InventoryPack.Features.Assets;

public static class AssetsEndpoints
{
    public static void MapAssetsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/assets", RegisterAssetsAsync)
            .WithTags("Assets")
            .WithSummary("Register physical asset identities under a code group");
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
}
