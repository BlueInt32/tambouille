using Microsoft.EntityFrameworkCore;
using Tambouille.Models;

namespace Tambouille.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Meal> Meals => Set<Meal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Date).IsRequired();
            entity.Property(m => m.Name).IsRequired();
            entity.HasIndex(m => new { m.Date, m.Slot }).IsUnique();
        });
    }
}
