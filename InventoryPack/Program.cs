using InventoryPack.Data;
using InventoryPack.Endpoints;
using InventoryPack.Features.Assets.Import;
using InventoryPack.Features.Assets.Import.Excel;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IAssetImporter, ExcelAssetImporter>();
builder.Services.AddScoped<ImportAssetsHandler>();

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

app.MapGet("/", () => "Running");
app.MapAssetImportEndpoints();

app.Run();