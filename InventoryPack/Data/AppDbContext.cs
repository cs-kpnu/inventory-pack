using InventoryPack.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<LedgerEntryCode> LedgerEntryCodes => Set<LedgerEntryCode>();
    public DbSet<CodeGroup> CodeGroups => Set<CodeGroup>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<RejectedRow> RejectedRows => Set<RejectedRow>();
    public DbSet<RejectedRowReason> RejectedRowReasons => Set<RejectedRowReason>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SourceTitle).HasMaxLength(4000);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Subaccount).HasMaxLength(32);
            entity.Property(e => e.Mvo).HasMaxLength(150);
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);

            entity.HasIndex(e => new { e.Mvo, e.Subaccount });
        });

        modelBuilder.Entity<CodeGroup>(entity =>
        {
            entity.HasKey(g => g.Id);

            entity.Property(g => g.Code).HasMaxLength(32);
            entity.HasIndex(g => g.Code).IsUnique();
        });

        modelBuilder.Entity<LedgerEntryCode>(entity =>
        {
            entity.HasKey(c => new { c.LedgerEntryId, c.CodeGroupId });

            entity.HasOne(c => c.LedgerEntry)
                .WithMany(e => e.Codes)
                .HasForeignKey(c => c.LedgerEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.CodeGroup)
                .WithMany(g => g.LedgerCodes)
                .HasForeignKey(c => c.CodeGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => c.CodeGroupId);
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.HasOne(a => a.CodeGroup)
                .WithMany(g => g.Assets)
                .HasForeignKey(a => a.CodeGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => a.CodeGroupId);
            entity.HasIndex(a => a.RequestId);
            entity.HasIndex(a => a.PrintedAt);
        });

        modelBuilder.Entity<RejectedRow>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.RawText).HasMaxLength(2000);
            entity.Property(r => r.Unit).HasMaxLength(50);
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