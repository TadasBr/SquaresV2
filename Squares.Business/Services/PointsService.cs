using Squares.Business.Exceptions;
using Squares.Business.Models;
using Squares.Persistence.Entities;
using Squares.Persistence.Repositories;
using System.Collections.Concurrent;

namespace Squares.Business.Services;

public class PointsService(IPointsRepository pointsRepository) : IPointsService
{
    private readonly IPointsRepository _pointsRepository = pointsRepository;

    private static List<Square>? _cachedSquares;
    private static HashSet<(int X, int Y)>? _cachedPointSet;
    private static readonly Lock _cacheLock = new();

    private static void InvalidateSquaresCache()
    {
        lock (_cacheLock)
        {
            _cachedSquares = null;
            _cachedPointSet = null;
        }
    }

    public async Task<List<Point>> GetAllPointsAsync()
        => await _pointsRepository.GetAllAsync();

    public async Task<Point?> GetPointByIdAsync(int id)
        => await _pointsRepository.GetByIdAsync(id);

    public async Task<Point> AddPointAsync(Point point)
    {
        bool exists = await _pointsRepository.ExistsAsync(point.X, point.Y);
        if (exists)
        {
            throw new DuplicatePointException(
                $"A point with coordinates ({point.X}, {point.Y}) already exists."
            );
        }

        await _pointsRepository.AddAsync(point);

        InvalidateSquaresCache();

        return point;
    }

    public async Task UpdatePointAsync(int id, Point point)
    {
        Point? existingPoint = await _pointsRepository.GetByIdAsync(id);
        if (existingPoint == null)
        {
            throw new InvalidOperationException($"Point with ID {id} not found.");
        }

        bool duplicateExists = await _pointsRepository.ExistsAsync(point.X, point.Y, excludeId: id);
        if (duplicateExists)
        {
            throw new DuplicatePointException($"A point with coordinates ({point.X}, {point.Y}) already exists.");
        }

        await _pointsRepository.UpdateAsync(id, point);

        InvalidateSquaresCache();
    }

    public async Task DeletePointAsync(int id)
    {
        Point? existingPoint = await _pointsRepository.GetByIdAsync(id);
        if (existingPoint == null)
        {
            throw new InvalidOperationException($"Point with ID {id} not found.");
        }

        await _pointsRepository.DeleteAsync(id);

        InvalidateSquaresCache();
    }

    public async Task<List<Point>> ImportPointsAsync(List<Point> points)
    {
        if (points == null || points.Count == 0)
        {
            throw new ArgumentException("Points list cannot be empty.", nameof(points));
        }

        List<Point> existingPoints = await _pointsRepository.GetAllAsync();
        HashSet<(int X, int Y)> existingSet = existingPoints.Select(p => (p.X, p.Y)).ToHashSet();

        List<Point> newPoints = points
            .Where(p => !existingSet.Contains((p.X, p.Y)))
            .ToList();

        if (newPoints.Count == 0)
        {
            throw new DuplicatePointException("All points already exist in the database.");
        }

        await _pointsRepository.AddRangeAsync(newPoints);

        InvalidateSquaresCache();

        return newPoints;
    }

    public async Task<List<Square>> IdentifySquaresAsync()
    {
        List<Point> points = await _pointsRepository.GetAllAsync();
        HashSet<(int X, int Y)> pointSet = points.Select(p => (p.X, p.Y)).ToHashSet();

        lock (_cacheLock)
        {
            if (_cachedSquares != null && _cachedPointSet != null && _cachedPointSet.SetEquals(pointSet))
            {
                return _cachedSquares;
            }
        }

        ConcurrentDictionary<string, bool> seenSquares = new ConcurrentDictionary<string, bool>();
        ConcurrentBag<Square> squaresBag = new ConcurrentBag<Square>();

        points
            .SelectMany((p1, i) => points.Skip(i + 1), (p1, p2) => (p1, p2))
            .AsParallel()
            .ForAll(pair =>
            {
                Point p1 = pair.p1;
                Point p2 = pair.p2;

                int dx = p2.X - p1.X;
                int dy = p2.Y - p1.Y;

                ((int X, int Y) p3, (int X, int Y) p4)[] candidates = new[]
                {
                    (p3: (X: p1.X - dy, Y: p1.Y + dx), p4: (X: p2.X - dy, Y: p2.Y + dx)),
                    (p3: (X: p1.X + dy, Y: p1.Y - dx), p4: (X: p2.X + dy, Y: p2.Y - dx))
                };

                foreach (((int X, int Y) p3, (int X, int Y) p4) c in candidates)
                {
                    if (pointSet.Contains(c.p3) && pointSet.Contains(c.p4))
                    {
                        Point[] squarePoints = new[]
                        {
                            p1,
                            p2,
                            new (c.p3.X, c.p3.Y),
                            new (c.p4.X, c.p4.Y)
                        };

                        string key = string.Join(";", squarePoints
                            .OrderBy(p => p.X).ThenBy(p => p.Y)
                            .Select(p => $"{p.X},{p.Y}"));

                        if (seenSquares.TryAdd(key, true))
                        {
                            squaresBag.Add(new Square { Points = squarePoints });
                        }
                    }
                }
            });

        List<Square> squares = squaresBag.ToList();

        lock (_cacheLock)
        {
            _cachedSquares = squares;
            _cachedPointSet = pointSet;
        }

        return squares;
    }
}
