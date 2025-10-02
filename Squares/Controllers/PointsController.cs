using Microsoft.AspNetCore.Mvc;
using Squares.API.Dtos;
using Squares.Business.Exceptions;
using Squares.Business.Services;
using System.Net;
using Point = Squares.Persistence.Entities.Point;

namespace Squares.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PointsController(IPointsService pointsService) : ControllerBase
{
    private readonly IPointsService _pointsService = pointsService;


    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAllPoints()
    {
        List<Point> points = await _pointsService.GetAllPointsAsync();

        return Ok(points);
    }


    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PointDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetPointById(int id)
    {
        if (id <= 0)
        {
            return BadRequest("ID must be greater than 0");
        }

        Point? point = await _pointsService.GetPointByIdAsync(id);

        return point != null ? Ok(point) : NotFound();
    }


    [HttpPost]
    [ProducesResponseType(typeof(PointDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreatePoint([FromBody] PointDto point)
    {
        try
        {
            Point createdPoint = await _pointsService.AddPointAsync(
                new Point(point.X!.Value, point.Y!.Value)
            );

            return CreatedAtAction(nameof(GetPointById), new { id = createdPoint.Id }, createdPoint);
        }
        catch (DuplicatePointException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PointDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UpdatePoint(int id, [FromBody] PointDto point)
    {
        if (id <= 0)
        {
            return BadRequest("ID must be greater than 0");
        }

        try
        {
            await _pointsService.UpdatePointAsync(id, new Point(point.X!.Value, point.Y!.Value));

            Point? updatedPoint = await _pointsService.GetPointByIdAsync(id);

            return Ok(updatedPoint);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (DuplicatePointException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> DeletePoint(int id)
    {
        if (id <= 0)
        {
            return BadRequest("ID must be greater than 0");
        }

        try
        {
            await _pointsService.DeletePointAsync(id);

            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }


    [HttpGet("squares")]
    [ProducesResponseType(typeof(List<SquareDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetSquares()
    {
        List<Squares.Business.Models.Square> squares = await _pointsService.IdentifySquaresAsync();

        List<SquareDto> result = squares
            .Select(s => new SquareDto(s))
            .ToList();

        return Ok(result);
    }


    [HttpPost("import")]
    [ProducesResponseType(typeof(ImportResultDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ImportPoints([FromBody] List<PointDto> points)
    {
        if (points == null || points.Count == 0)
        {
            return BadRequest("At least one point is required");
        }

        try
        {
            List<Point> pointEntities = points
                .Select(p => new Point(p.X!.Value, p.Y!.Value))
                .ToList();

            List<Point> importedPoints = await _pointsService.ImportPointsAsync(pointEntities);

            ImportResultDto result = new()
            {
                ImportedCount = importedPoints.Count,
                TotalSubmitted = points.Count,
                SkippedCount = points.Count - importedPoints.Count,
                ImportedPoints = importedPoints
            };

            return CreatedAtAction(nameof(GetAllPoints), result);
        }
        catch (DuplicatePointException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
