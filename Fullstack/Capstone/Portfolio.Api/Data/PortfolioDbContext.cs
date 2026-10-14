using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Models;

namespace Portfolio.Api.Data;

public sealed class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : DbContext(options)
{
    public DbSet<PortfolioUser> PortfolioUsers => Set<PortfolioUser>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Skill> Skills => Set<Skill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PortfolioUser>()
            .HasIndex(user => user.Email)
            .IsUnique()
            .HasDatabaseName("IX_PortfolioUsers_Email");

        modelBuilder.Entity<PortfolioUser>()
            .Property(user => user.Email)
            .UseCollation("NOCASE");

        modelBuilder.Entity<Project>()
            .HasOne(project => project.PortfolioUser)
            .WithMany(user => user.Projects)
            .HasForeignKey(project => project.PortfolioUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PortfolioUser>()
            .HasMany(user => user.Skills)
            .WithMany(skill => skill.PortfolioUsers);

        modelBuilder.Entity<Skill>()
            .HasIndex(skill => skill.Name)
            .IsUnique()
            .HasDatabaseName("IX_Skills_Name");

        modelBuilder.Entity<Skill>()
            .Property(skill => skill.Name)
            .UseCollation("NOCASE");
    }
}