using Squares.Business.Models;

namespace Squares.API.Dtos;

public class SquareDto 
{
    public PointDto[] Points { get; set; }

    public SquareDto(Square square)
    {
        Points = square.Points.Select(p => new PointDto { X = p.X, Y = p.Y }).ToArray();
    }
}
