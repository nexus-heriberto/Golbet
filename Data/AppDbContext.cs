// Golbet/Data/AppDbContext.cs

using Golbet.Common;
using Golbet.Entities;
using Microsoft.EntityFrameworkCore;

namespace Golbet.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<Match> Matches => Set<Match>();

    public DbSet<Bet> Bets => Set<Bet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Team names must be unique,
        // case-insensitive and accent-insensitive.
        modelBuilder.Entity<Team>()
            .Property(t => t.Name)
            .UseCollation("SQL_Latin1_General_CP1_CI_AI");

        modelBuilder.Entity<Team>()
            .HasIndex(t => t.Name)
            .IsUnique();

        // Match -> HomeTeam
        modelBuilder.Entity<Match>()
            .HasOne(m => m.HomeTeam)
            .WithMany()
            .HasForeignKey(m => m.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Match -> AwayTeam
        modelBuilder.Entity<Match>()
            .HasOne(m => m.AwayTeam)
            .WithMany()
            .HasForeignKey(m => m.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Bet -> Match
        modelBuilder.Entity<Bet>()
            .HasOne(b => b.Match)
            .WithMany(m => m.Bets)
            .HasForeignKey(b => b.MatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = utcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedDate = utcNow;

                    // CreatedDate must never change.
                    entry.Property(e => e.CreatedDate).IsModified = false;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}