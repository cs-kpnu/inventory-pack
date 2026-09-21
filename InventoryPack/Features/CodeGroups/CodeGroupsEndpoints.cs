namespace InventoryPack.Features.CodeGroups;

public static class CodeGroupsEndpoints
{
    public static void MapCodeGroupsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/code-groups")
            .WithTags("CodeGroups");

        group.MapGet("/", async (
                string? search,
                int? page,
                int? pageSize,
                CodeGroupQueryHandler handler,
                CancellationToken ct) =>
            {
                var pageNumber = page ?? 1;
                var effectivePageSize = pageSize ?? 25;

                if (pageNumber < 1 || effectivePageSize < 1 || effectivePageSize > 100)
                    return Results.BadRequest("Page must be >= 1 and pageSize must be between 1 and 100.");

                var response = await handler.GetPagedAsync(search, pageNumber, effectivePageSize, ct);
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
    }
}