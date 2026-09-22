namespace InventoryPack.Features.LedgerEntries;

public static class LedgerEntriesEndpoints
{
    public static void MapLedgerEntriesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/ledger-entries", GetPagedAsync)
            .WithTags("LedgerEntries")
            .WithSummary("Get paginated accountant source rows with their codes");

        app.MapGet("/api/ledger-entries/{id:guid}", GetByIdAsync)
            .WithTags("LedgerEntries")
            .WithSummary("Get a source row with its codes and registered assets");
    }

    private static async Task<IResult> GetPagedAsync(
        string? search,
        int? page,
        int? pageSize,
        LedgerEntryQueryHandler handler,
        CancellationToken ct)
    {
        var pageNumber = page ?? 1;
        var effectivePageSize = pageSize ?? 25;

        if (pageNumber < 1 || effectivePageSize is < 1 or > 100)
            return Results.BadRequest("Page must be >= 1 and pageSize must be between 1 and 100.");

        var response = await handler.GetPagedAsync(search, pageNumber, effectivePageSize, ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        LedgerEntryQueryHandler handler,
        CancellationToken ct)
    {
        var detail = await handler.GetByIdAsync(id, ct);
        return detail is not null ? Results.Ok(detail) : Results.NotFound();
    }
}