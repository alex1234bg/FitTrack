using FitTrack.Controllers;
using Xunit;
using FitTrack.Data;
using FitTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FitTrack.Tests;

public class HomeControllerTests
{
    private static ApplicationDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new ApplicationDbContext(options);
    }

    // Index() should return a ViewResult
    [Fact]
    public async Task Index_ReturnsViewResult()
    {
        var db = CreateDb(nameof(Index_ReturnsViewResult));
        var controller = new HomeController(Mock.Of<ILogger<HomeController>>(), db);

        var result = await controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    // TotalPrograms should be 0 when the database is empty
    [Fact]
    public async Task Index_EmptyDatabase_TotalProgramsIsZero()
    {
        var db = CreateDb(nameof(Index_EmptyDatabase_TotalProgramsIsZero));
        var controller = new HomeController(Mock.Of<ILogger<HomeController>>(), db);

        await controller.Index();

        Assert.Equal(0, controller.ViewBag.TotalPrograms);
    }

    // Featured programs should be at most 3
    [Fact]
    public async Task Index_WithFivePrograms_FeaturesAtMostThree()
    {
        var db = CreateDb(nameof(Index_WithFivePrograms_FeaturesAtMostThree));
        for (int i = 1; i <= 5; i++)
        {
            db.WorkoutPrograms.Add(new WorkoutProgram
            {
                Name          = $"Program {i}",
                Description   = "Test",
                Level         = Level.Beginner,
                Category      = Category.Strength,
                DurationWeeks = 4,
                CreatedOn     = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();

        var controller = new HomeController(Mock.Of<ILogger<HomeController>>(), db);
        await controller.Index();

        var featured = (List<WorkoutProgram>)controller.ViewBag.FeaturedPrograms;
        Assert.True(featured.Count <= 3);
    }
}
