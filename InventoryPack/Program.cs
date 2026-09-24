using InventoryPack.Data;
using InventoryPack.Endpoints;
using InventoryPack.Features.Assets;
using InventoryPack.Features.Assets.Handlers;
using InventoryPack.Features.Assets.Import;
using InventoryPack.Features.Assets.Import.Excel;
using InventoryPack.Features.LedgerEntries;
using InventoryPack.Features.LedgerEntries.Handlers;
using InventoryPack.Features.RejectedRows;
using InventoryPack.Features.RejectedRows.Handlers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddKeyedSingleton<IAssetImporter, ExcelAssetImporter>(".xlsx");
builder.Services.AddScoped<ImportAssetsHandler>();
builder.Services.AddScoped<RegisterAssetsHandler>();
builder.Services.AddScoped<GetLedgerEntriesHandler>();
builder.Services.AddScoped<GetLedgerEntryHandler>();
builder.Services.AddScoped<GetRejectedRowsHandler>();
builder.Services.AddScoped<GetRejectedRowHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapHealthEndpoints();
}
else
{
    app.UseHttpsRedirection();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.MapAssetsEndpoints();
app.MapLedgerEntriesEndpoints();
app.MapRejectedRowsEndpoints();

app.Run();
