using InventoryPack.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<RejectedRow> RejectedRows => Set<RejectedRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.InventoryNumber).HasMaxLength(32);
            entity.Property(a => a.Name).HasMaxLength(500);
            entity.Property(a => a.Mvo).HasMaxLength(150);
            entity.Property(a => a.Subaccount).HasMaxLength(32);

            entity.HasIndex(a => a.InventoryNumber).IsUnique();
            entity.HasIndex(a => new { a.Mvo, a.Subaccount });
            entity.HasIndex(a => a.PrintedAt);
        });

        modelBuilder.Entity<RejectedRow>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.RawText).HasMaxLength(2000);
            entity.Property(r => r.Reason).HasConversion<string>().HasMaxLength(50);
            entity.Property(r => r.Subaccount).HasMaxLength(32);
            entity.Property(r => r.Mvo).HasMaxLength(150);

            entity.HasIndex(r => r.RowNumber);
        });
    }
}