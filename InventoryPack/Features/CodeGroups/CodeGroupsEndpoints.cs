namespace InventoryPack.Features.CodeGroups;

public static class CodeGroupsEndpoints
{
    public static void MapCodeGroupsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/code-groups")
            .WithTags("CodeGroups");

        group.MapGet("/", async (
                string? search,
                string? status,
                int? page,
                int? pageSize,
                CodeGroupQueryHandler handler,
                CancellationToken ct) =>
            {
                var pageNumber = page ?? 1;
                var effectivePageSize = pageSize ?? 25;

                if (pageNumber < 1 || effectivePageSize < 1 || effectivePageSize > 100)
                    return Results.BadRequest("Page must be >= 1 and pageSize must be between 1 and 100.");

                var response = await handler.GetPagedAsync(search, status, pageNumber, effectivePageSize, ct);
                return Results.Ok(response);
            })
            .WithSummary("Get paginated list of code groups");

        group.MapGet("/{id:guid}", async (
                Guid id,
                CodeGroupQueryHandler handler,
                CancellationToken ct) =>
            {
                var detail = await handler.GetByIdAsync(id, ct);
                return detail is not null
                    ? Results.Ok(detail)
                    : Results.NotFound();
            })
            .WithSummary("Get code group detail with contributing source rows");

        group.MapPost("/{id:guid}/assets", async (
                Guid id,
                RegisterAssetsRequest request,
                AssetRegistrationHandler handler,
                CancellationToken ct) =>
            {
                var result = await handler.RegisterAsync(id, request, ct);
                return result switch
                {
                    RegisterAssetsResult.Success s => Results.Created($"/api/code-groups/{id}", s.Assets),
                    RegisterAssetsResult.IdempotentReplay r => Results.Ok(r.Assets),
                    RegisterAssetsResult.NotFound nf => Results.NotFound(new { error = nf.Message }),
                    RegisterAssetsResult.Conflict cf => Results.Conflict(new { error = cf.Message }),
                    RegisterAssetsResult.BadRequest br => Results.BadRequest(new { error = br.Message }),
                    _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
                };
            })
            .WithSummary("Register physical asset identities under a code group");
    }
}