using System.Data;
using InventoryPack.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryPack.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        var conn = Database.GetDbConnection();
        if (conn.State == ConnectionState.Open)
            SqliteCustomFunctions.Register(conn);
    }

    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<LedgerEntryCode> LedgerEntryCodes => Set<LedgerEntryCode>();
    public DbSet<CodeGroup> CodeGroups => Set<CodeGroup>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<RejectedRow> RejectedRows => Set<RejectedRow>();
    public DbSet<RejectionIssue> RejectionIssues => Set<RejectionIssue>();
    public DbSet<RejectedRowIssue> RejectedRowIssues => Set<RejectedRowIssue>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.AddInterceptors(SqliteConnectionInterceptor.Instance);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .HasDbFunction(typeof(SqliteCustomFunctions).GetMethod(nameof(SqliteCustomFunctions.ContainsIgnoreCase))!)
            .HasName("contains_ignore_case");

        modelBuilder
            .HasDbFunction(typeof(SqliteCustomFunctions).GetMethod(nameof(SqliteCustomFunctions.EqualsIgnoreCase))!)
            .HasName("equals_ignore_case");

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

            entity.Property(a => a.RegisteredForMvo).HasMaxLength(150);

            entity.HasOne(a => a.CodeGroup)
                .WithMany(g => g.Assets)
                .HasForeignKey(a => a.CodeGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => a.CodeGroupId);
            entity.HasIndex(a => a.RequestId);
        });

        modelBuilder.Entity<RejectedRow>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.RawText).HasMaxLength(2000);
            entity.Property(r => r.ParsedName).HasMaxLength(500);
            entity.Property(r => r.Unit).HasMaxLength(50);
            entity.Property(r => r.Subaccount).HasMaxLength(32);
            entity.Property(r => r.Mvo).HasMaxLength(150);
            entity.Property(r => r.Quantity).HasPrecision(18, 4);

            entity.HasIndex(r => r.RowNumber);
        });

        modelBuilder.Entity<RejectionIssue>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Reason).HasConversion<string>().HasMaxLength(50);
            entity.Property(i => i.Code).HasMaxLength(32);
            entity.HasIndex(i => i.Reason);
        });

        modelBuilder.Entity<RejectedRowIssue>(entity =>
        {
            entity.HasKey(link => new { link.RejectedRowId, link.RejectionIssueId });

            entity.HasOne(link => link.RejectedRow)
                .WithMany(row => row.Issues)
                .HasForeignKey(link => link.RejectedRowId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(link => link.RejectionIssue)
                .WithMany(issue => issue.Rows)
                .HasForeignKey(link => link.RejectionIssueId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(link => link.RejectionIssueId);
        });
    }
}
