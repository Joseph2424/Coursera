using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Data;

public class CounterDbContext(DbContextOptions<CounterDbContext> options) : DbContext(options)
{
    public DbSet<Counter> Counters => Set<Counter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Counter>()
            .HasKey(c => c.Name);
    }
}
