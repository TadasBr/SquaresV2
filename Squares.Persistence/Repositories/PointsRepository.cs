using Microsoft.EntityFrameworkCore;
using Squares.Persistence.Entities;

namespace Squares.Persistence.Repositories;

public class PointsRepository(SquaresDbContext context) : IPointsRepository
{
    private readonly SquaresDbContext _context = context;

    public async Task<List<Point>> GetAllAsync()
    {
        return await _context.Points.ToListAsync();
    }

    public async Task<Point?> GetByIdAsync(int id)
    {
        return await _context.Points.FindAsync(id);
    }

    public async Task AddAsync(Point point)
    {
        await _context.Points.AddAsync(point);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Point> points)
    {
        await _context.Points.AddRangeAsync(points);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, Point point)
    {
        Point? existingPoint = await _context.Points.FindAsync(id);

        if (existingPoint != null)
        {
            existingPoint.X = point.X;
            existingPoint.Y = point.Y;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        Point? point = await _context.Points.FindAsync(id);

        if (point != null)
        {
            _context.Points.Remove(point);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int x, int y, int? excludeId = null)
    {
        IQueryable<Point> query = _context.Points.Where(p => p.X == x && p.Y == y);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
