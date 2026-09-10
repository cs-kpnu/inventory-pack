using InventoryPack.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/health");

        group.MapGet("/db", async (AppDbContext db) =>
        {
            var canConnect = await db.Database.CanConnectAsync();
            var applied = (await db.Database.GetAppliedMigrationsAsync()).ToList();
            var assetCount = await db.Assets.CountAsync();
            var rejectedCount = await db.RejectedRows.CountAsync();

            return Results.Ok(new
            {
                provider = db.Database.ProviderName,
                connected = canConnect,
                appliedMigrations = applied,
                assets = assetCount,
                rejectedRows = rejectedCount
            });
        });
    }
}