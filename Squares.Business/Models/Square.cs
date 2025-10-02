using Squares.Persistence.Entities;

namespace Squares.Business.Models;

public class Square
{
    public Point[] Points { get; set; } = new Point[4];
}