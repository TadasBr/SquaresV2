namespace Squares.Persistence.UnitTests;

using Microsoft.EntityFrameworkCore;
using Squares.Persistence.Entities;

namespace Squares.Persistence.Repositories;

public class PointsRepository(SquaresDbContext context) : IPointsRepository
{
    private readonly SquaresDbContext _context = context;

    public async Task AddAsync(Point point)
    {
        bool exists = await _context.Points
            .AnyAsync(p => p.X == point.X && p.Y == point.Y);

        if (exists)
        {
            throw new InvalidOperationException("A point with the same coordinates already exists.");
        }

        await _context.Points.AddAsync(point);
        await _context.SaveChangesAsync();
    }


    public async Task DeleteAsync(int id)
    {
        var point = await _context.Points.FindAsync(id) ?? throw new InvalidOperationException($"Point with ID {id} not found");
        _context.Points.Remove(point);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Point>> GetAllAsync()
    {
        return await _context.Points.ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Point> points)
    {
        var existingPoints = await _context.Points
            .Select(p => new { p.X, p.Y })
            .ToListAsync();

        var existingSet = existingPoints
            .Select(p => (p.X, p.Y))
            .ToHashSet();

        var newPoints = points
            .Where(p => !existingSet.Contains((p.X, p.Y)))
            .ToList();

        if (newPoints.Count == 0)
        {
            throw new InvalidOperationException("All points in list already exist.");
        }

        await _context.Points.AddRangeAsync(newPoints);
        await _context.SaveChangesAsync();
    }

    public async Task<Point?> FindByCoordinatesAsync(int x, int y)
    {
        return await _context.Points.FirstOrDefaultAsync(p => p.X == x && p.Y == y);
    }
}


