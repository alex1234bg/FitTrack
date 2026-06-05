using FitTrack.Controllers;
using Xunit;
using FitTrack.Data;
using FitTrack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace FitTrack.Tests;

public class WorkoutProgramsControllerTests
{
    private static ApplicationDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var um = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
        um.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);
        return um;
    }

    private static WorkoutProgramsController CreateController(ApplicationDbContext db)
    {
        var controller = new WorkoutProgramsController(db, CreateUserManager().Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    // Index() should return a ViewResult
    [Fact]
    public async Task Index_ReturnsViewResult()
    {
        var controller = CreateController(CreateDb(nameof(Index_ReturnsViewResult)));

        var result = await controller.Index(null, null);

        Assert.IsType<ViewResult>(result);
    }

    // Filtering by Level should return only matching programs
    [Fact]
    public async Task Index_FilterByLevel_ReturnsOnlyMatchingPrograms()
    {
        var db = CreateDb(nameof(Index_FilterByLevel_ReturnsOnlyMatchingPrograms));
        db.WorkoutPrograms.AddRange(
            new WorkoutProgram { Name = "Beginner", Description = "D", Level = Level.Beginner,  Category = Category.Strength, DurationWeeks = 4, CreatedOn = DateTime.UtcNow },
            new WorkoutProgram { Name = "Advanced",  Description = "D", Level = Level.Advanced,  Category = Category.Strength, DurationWeeks = 8, CreatedOn = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var result = await CreateController(db).Index(Level.Beginner, null);

        var view  = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<WorkoutProgram>>(view.Model);
        Assert.Single(model);
        Assert.Equal("Beginner", model[0].Name);
    }

    // Details() with a valid id should return a ViewResult
    [Fact]
    public async Task Details_WithValidId_ReturnsViewResult()
    {
        var db = CreateDb(nameof(Details_WithValidId_ReturnsViewResult));
        var program = new WorkoutProgram { Name = "Test", Description = "D", Level = Level.Beginner, Category = Category.Strength, DurationWeeks = 4, CreatedOn = DateTime.UtcNow };
        db.WorkoutPrograms.Add(program);
        await db.SaveChangesAsync();

        var result = await CreateController(db).Details(program.Id);

        Assert.IsType<ViewResult>(result);
    }

    // Details() with a non-existent id should return NotFound
    [Fact]
    public async Task Details_WithInvalidId_ReturnsNotFound()
    {
        var result = await CreateController(CreateDb(nameof(Details_WithInvalidId_ReturnsNotFound))).Details(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
