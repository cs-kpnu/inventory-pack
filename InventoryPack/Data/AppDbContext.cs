using InventoryPack.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<RejectedRow> RejectedRows => Set<RejectedRow>();
    public DbSet<RejectedRowReason> RejectedRowReasons => Set<RejectedRowReason>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Subaccount).HasMaxLength(32);
            entity.Property(e => e.Mvo).HasMaxLength(150);
            entity.Property(e => e.Name).HasMaxLength(500);

            entity.HasIndex(e => new { e.Mvo, e.Subaccount });
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.InventoryNumber).HasMaxLength(32);

            entity.HasOne(a => a.LedgerEntry)
                .WithMany(e => e.Assets)
                .HasForeignKey(a => a.LedgerEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => new { a.InventoryNumber, a.UnitIndex }).IsUnique();
            entity.HasIndex(a => a.InventoryNumber);
            entity.HasIndex(a => a.PrintedAt);
        });

        modelBuilder.Entity<RejectedRow>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.RawText).HasMaxLength(2000);
            entity.Property(r => r.Subaccount).HasMaxLength(32);
            entity.Property(r => r.Mvo).HasMaxLength(150);
            entity.Property(r => r.Quantity).HasPrecision(18, 4);

            entity.HasMany(r => r.Reasons)
                .WithOne(re => re.RejectedRow)
                .HasForeignKey(re => re.RejectedRowId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => r.RowNumber);
        });

        modelBuilder.Entity<RejectedRowReason>(entity =>
        {
            entity.HasKey(re => new { re.RejectedRowId, re.Reason });
            entity.Property(re => re.Reason).HasConversion<string>().HasMaxLength(50);
            entity.HasIndex(re => re.Reason);
        });
    }
}