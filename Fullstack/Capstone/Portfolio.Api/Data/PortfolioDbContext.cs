using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Portfolio.Api.Models;

namespace Portfolio.Api.Data;

public sealed class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<PortfolioUser> PortfolioUsers => Set<PortfolioUser>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<PortfolioUserSkill> PortfolioUserSkills => Set<PortfolioUserSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
            .HasOne(user => user.Owner)
            .WithMany(owner => owner.Portfolios)
            .HasForeignKey(user => user.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PortfolioUserSkill>(entity =>
        {
            entity.ToTable("PortfolioUserSkill");
            entity.HasKey(link => new { link.PortfolioUsersId, link.SkillsId });
            entity.HasOne(link => link.PortfolioUser)
                .WithMany(user => user.SkillLinks)
                .HasForeignKey(link => link.PortfolioUsersId);
            entity.HasOne(link => link.Skill)
                .WithMany(skill => skill.PortfolioUserLinks)
                .HasForeignKey(link => link.SkillsId);
        });

        modelBuilder.Entity<Skill>()
            .HasIndex(skill => skill.Name)
            .IsUnique()
            .HasDatabaseName("IX_Skills_Name");

        modelBuilder.Entity<Skill>()
            .Property(skill => skill.Name)
            .UseCollation("NOCASE");
    }
}