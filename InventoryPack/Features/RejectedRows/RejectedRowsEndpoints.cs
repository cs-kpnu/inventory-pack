using InventoryPack.Data.Entities;
using InventoryPack.Features.RejectedRows.Handlers;

namespace InventoryPack.Features.RejectedRows;

public static class RejectedRowsEndpoints
{
    public static void MapRejectedRowsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/rejected-rows", GetPagedAsync)
            .WithTags("RejectedRows")
            .WithSummary("Get paginated rejected rows with search and reason filtering");

        app.MapGet("/api/rejected-rows/{id:guid}", GetByIdAsync)
            .WithTags("RejectedRows")
            .WithSummary("Get a rejected row by its ID");
    }

    private static async Task<IResult> GetPagedAsync(
        string? search,
        string? reason,
        int? page,
        int? pageSize,
        GetRejectedRowsHandler handler,
        CancellationToken ct)
    {
        var pageNumber = page ?? 1;
        var effectivePageSize = pageSize ?? 25;

        if (pageNumber < 1 || effectivePageSize is < 1 or > 100)
            return Results.BadRequest("Page must be >= 1 and pageSize must be between 1 and 100.");

        RejectionReason? filterReason = null;
        if (!string.IsNullOrWhiteSpace(reason) &&
            !string.Equals(reason.Trim(), "all", StringComparison.OrdinalIgnoreCase))
        {
            if (!Enum.TryParse<RejectionReason>(reason.Trim(), true, out var parsed) || !Enum.IsDefined(parsed))
                return Results.BadRequest(
                    $"Reason must be 'all' or one of: {string.Join(", ", Enum.GetNames<RejectionReason>())}.");
            filterReason = parsed;
        }

        var response = await handler.HandleAsync(search, filterReason, pageNumber, effectivePageSize, ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetRejectedRowHandler handler,
        CancellationToken ct)
    {
        var row = await handler.HandleAsync(id, ct);
        return row is not null ? Results.Ok(row) : Results.NotFound();
    }
}
