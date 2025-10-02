using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Squares.Business.Services;
using Squares.Persistence;
using Squares.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<SquaresDbContext>(options =>
    options.UseInMemoryDatabase(databaseName: "SquaresDB"));

builder.Services.AddScoped<IPointsRepository, PointsRepository>();
builder.Services.AddScoped<IPointsService, PointsService>();

builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Squares API",
        Version = "v2",
        Description = "API for managing points and identifying squares",
    });
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SquaresDbContext>();
    context.Database.EnsureCreated();
}

app.Run();