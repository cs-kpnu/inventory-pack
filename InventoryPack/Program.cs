using InventoryPack.Data;
using InventoryPack.Endpoints;
using InventoryPack.Features.Assets;
using InventoryPack.Features.Assets.Import;
using InventoryPack.Features.Assets.Import.Excel;
using InventoryPack.Features.LedgerEntries;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddKeyedSingleton<IAssetImporter, ExcelAssetImporter>(".xlsx");
builder.Services.AddScoped<ImportAssetsHandler>();
builder.Services.AddScoped<AssetRegistrationHandler>();
builder.Services.AddScoped<LedgerEntryQueryHandler>();

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

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapAssetImportEndpoints();
app.MapAssetsEndpoints();
app.MapLedgerEntriesEndpoints();
app.MapFallbackToFile("index.html");

app.Run();