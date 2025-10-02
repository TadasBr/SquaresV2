using Moq;
using Squares.Business.Services;
using Squares.Persistence.Entities;
using Squares.Persistence.Repositories;

namespace Squares.Business.UnitTests;

[TestFixture]
public class PointsServiceTests
{
    private Mock<IPointsRepository> _pointsRepositoryMock = null!;
    private PointsService _pointsService = null!;

    [SetUp]
    public void Setup()
    {
        _pointsRepositoryMock = new Mock<IPointsRepository>();
        _pointsService = new PointsService(_pointsRepositoryMock.Object);
    }

    [Test]
    public async Task GetAllPointsAsync_ShouldReturnAllPoints()
    {
        var points = new List<Point>
        {
            new(1, 1),
            new(2, 2)
        };
        _pointsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(points);

        var result = await _pointsService.GetAllPointsAsync();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].X, Is.EqualTo(1));
        Assert.That(result[1].X, Is.EqualTo(2));
    }

    [Test]
    public async Task GetPointByIdAsync_ShouldReturnPoint()
    {
        var point = new Point(3, 3);
        _pointsRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(point);

        var result = await _pointsService.GetPointByIdAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.X, Is.EqualTo(3));
    }

    [Test]
    public async Task AddPointAsync_ShouldAddPoint_WhenNotDuplicate()
    {
        var newPoint = new Point(5, 5);
        _pointsRepositoryMock.Setup(r => r.ExistsAsync(5, 5, null)).ReturnsAsync(false);
        _pointsRepositoryMock.Setup(r => r.AddAsync(newPoint)).Returns(Task.CompletedTask);

        var result = await _pointsService.AddPointAsync(newPoint);

        Assert.That(result.X, Is.EqualTo(5));
        _pointsRepositoryMock.Verify(r => r.AddAsync(newPoint), Times.Once);
    }

    [Test]
    public async Task UpdatePointAsync_ShouldUpdatePoint_WhenValid()
    {
        var updatedPoint = new Point(6, 6);
        _pointsRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Point(0, 0));
        _pointsRepositoryMock.Setup(r => r.ExistsAsync(6, 6, 1)).ReturnsAsync(false);
        _pointsRepositoryMock.Setup(r => r.UpdateAsync(1, updatedPoint)).Returns(Task.CompletedTask);

        await _pointsService.UpdatePointAsync(1, updatedPoint);

        _pointsRepositoryMock.Verify(r => r.UpdateAsync(1, updatedPoint), Times.Once);
    }

    [Test]
    public async Task DeletePointAsync_ShouldDeletePoint_WhenExists()
    {
        var existingPoint = new Point(7, 7);
        _pointsRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingPoint);
        _pointsRepositoryMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        await _pointsService.DeletePointAsync(1);

        _pointsRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Test]
    public async Task ImportPointsAsync_ShouldAddNewPoints()
    {
        var newPoints = new List<Point> { new(8, 8) };
        _pointsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Point>());
        _pointsRepositoryMock.Setup(r => r.AddRangeAsync(newPoints)).Returns(Task.CompletedTask);

        var result = await _pointsService.ImportPointsAsync(newPoints);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].X, Is.EqualTo(8));
        _pointsRepositoryMock.Verify(r => r.AddRangeAsync(newPoints), Times.Once);
    }

    [Test]
    public async Task IdentifySquaresAsync_ShouldReturnSquares()
    {
        var points = new List<Point>
        {
            new(-1, 1),
            new(1, 1),
            new(1, -1),
            new(-1, -1)
        };

        _pointsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(points);

        var squares = await _pointsService.IdentifySquaresAsync();

        Assert.That(squares.Count, Is.EqualTo(1));
        var squarePoints = squares[0].Points.Select(p => (p.X, p.Y)).ToList();
        Assert.That(squarePoints, Has.Member((-1, 1)));
        Assert.That(squarePoints, Has.Member((1, -1)));
    }

    [Test]
    public async Task IdentifySquaresAsync_ShouldReturnOneSquare_WhenPointsFormASquare()
    {
        var points = new List<Point>
        {
            new(-1, 1),
            new(1, 1),
            new(1, -1),
            new(-1, -1)
        };

        _pointsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(points);

        var squares = await _pointsService.IdentifySquaresAsync();

        Assert.That(squares.Count, Is.EqualTo(1));
        var squarePoints = squares[0].Points.Select(p => (p.X, p.Y)).ToList();
        Assert.That(squarePoints, Has.Member((-1, 1)));
        Assert.That(squarePoints, Has.Member((1, -1)));
    }

    [Test]
    public async Task IdentifySquaresAsync_ShouldReturnNoSquares_WhenPointsDoNotFormSquare()
    {
        var points = new List<Point>
        {
            new(0, 0),
            new(1, 2),
            new(2, 1),
            new(3, 3)
        };

        _pointsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(points);

        var squares = await _pointsService.IdentifySquaresAsync();

        Assert.That(squares.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task IdentifySquaresAsync_ShouldHandleMultipleSquares()
    {
        var points = new List<Point>
        {
            // First square
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(1, 1),
            // Second square
            new(2, 2),
            new(2, 3),
            new(3, 2),
            new(3, 3)
        };

        _pointsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(points);

        var squares = await _pointsService.IdentifySquaresAsync();

        Assert.That(squares.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task IdentifySquaresAsync_ShouldReturnEmpty_WhenNoPoints()
    {
        _pointsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Point>());

        var squares = await _pointsService.IdentifySquaresAsync();

        Assert.That(squares.Count, Is.EqualTo(0));
    }
}


