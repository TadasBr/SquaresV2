using Squares.Persistence.Entities;

namespace Squares.API.Dtos;

public class ImportResultDto
{
    public int ImportedCount { get; set; }
    public int TotalSubmitted { get; set; }
    public int SkippedCount { get; set; }
    public List<Point> ImportedPoints { get; set; } = new();
}