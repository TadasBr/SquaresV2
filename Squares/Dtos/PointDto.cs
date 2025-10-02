using System.ComponentModel.DataAnnotations;

namespace Squares.API.Dtos;

public class PointDto
{
    [Required]
    public int? X { get; set; }

    [Required]
    public int? Y { get; set; }
}
