using Squares.Business.Models;
using Squares.Persistence.Entities;

namespace Squares.Business.Services;

public interface IPointsService
{
    Task<List<Point>> GetAllPointsAsync();
    Task<Point?> GetPointByIdAsync(int id);
    Task<Point> AddPointAsync(Point point);
    Task UpdatePointAsync(int id, Point point);
    Task DeletePointAsync(int id);
    Task<List<Point>> ImportPointsAsync(List<Point> points);
    Task<List<Square>> IdentifySquaresAsync();
}