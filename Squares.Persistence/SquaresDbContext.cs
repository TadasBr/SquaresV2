using Microsoft.EntityFrameworkCore;
using Squares.Persistence.Entities;

namespace Squares.Persistence;

public class SquaresDbContext(DbContextOptions<SquaresDbContext> options) : DbContext(options)
{
    public DbSet<Point> Points { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Point>(entity =>
        {
            entity.HasKey(p => p.Id);
        });

        modelBuilder.Entity<Point>().HasData(
            new Point(0, 0) { Id = 1 },
            new Point(1, 0) { Id = 2 },
            new Point(0, 1) { Id = 3 },
            new Point(1, 1) { Id = 4 },
            new Point(2, 0) { Id = 5 },
            new Point(2, 1) { Id = 6 },
            new Point(0, 2) { Id = 7 },
            new Point(1, 2) { Id = 8 }
        );
    }
}