using InventoryPack.Features.LedgerEntries.Handlers;

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
        string? status,
        int? page,
        int? pageSize,
        GetLedgerEntriesHandler handler,
        CancellationToken ct)
    {
        var pageNumber = page ?? 1;
        var effectivePageSize = pageSize ?? 25;

        if (pageNumber < 1 || effectivePageSize is < 1 or > 100)
            return Results.BadRequest("Page must be >= 1 and pageSize must be between 1 and 100.");

        LedgerEntryStatus? filterStatus = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            !string.Equals(status.Trim(), "all", StringComparison.OrdinalIgnoreCase))
        {
            if (!Enum.TryParse<LedgerEntryStatus>(status.Trim(), true, out var parsed))
                return Results.BadRequest(
                    $"Status must be 'all' or one of: {string.Join(", ", Enum.GetNames<LedgerEntryStatus>().Select(s => s.ToLowerInvariant()))}.");
            filterStatus = parsed;
        }

        var response = await handler.HandleAsync(search, filterStatus, pageNumber, effectivePageSize, ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        GetLedgerEntryHandler handler,
        CancellationToken ct)
    {
        var detail = await handler.HandleAsync(id, ct);
        return detail is not null ? Results.Ok(detail) : Results.NotFound();
    }
}
