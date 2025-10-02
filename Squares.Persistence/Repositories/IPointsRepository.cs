using Squares.Persistence.Entities;

namespace Squares.Persistence.Repositories;

public interface IPointsRepository
{
    Task<List<Point>> GetAllAsync();
    Task<Point?> GetByIdAsync(int id);
    Task AddAsync(Point point);
    Task AddRangeAsync(IEnumerable<Point> points);
    Task UpdateAsync(int id, Point point);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int x, int y, int? excludeId = null);
}