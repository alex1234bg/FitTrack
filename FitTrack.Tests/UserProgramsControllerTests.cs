using FitTrack.Controllers;
using Xunit;
using FitTrack.Data;
using FitTrack.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace FitTrack.Tests;

public class UserProgramsControllerTests
{
    private const string TestUserId = "test-user-id";

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
        um.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
        return um;
    }

    private static UserProgramsController CreateController(ApplicationDbContext db)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, TestUserId)
            }, "Test"))
        };

        var controller = new UserProgramsController(db, CreateUserManager().Object);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        return controller;
    }

    // Enroll() with a non-existent program id should return NotFound
    [Fact]
    public async Task Enroll_WithInvalidProgramId_ReturnsNotFound()
    {
        var result = await CreateController(CreateDb(nameof(Enroll_WithInvalidProgramId_ReturnsNotFound))).Enroll(999);

        Assert.IsType<NotFoundResult>(result);
    }

    // Enroll() should add a UserProgram record to the database
    [Fact]
    public async Task Enroll_WithValidProgram_AddsUserProgramToDatabase()
    {
        var db = CreateDb(nameof(Enroll_WithValidProgram_AddsUserProgramToDatabase));
        var program = new WorkoutProgram { Name = "Test", Description = "D", Level = Level.Beginner, Category = Category.Strength, DurationWeeks = 4, CreatedOn = DateTime.UtcNow };
        db.WorkoutPrograms.Add(program);
        await db.SaveChangesAsync();

        await CreateController(db).Enroll(program.Id);

        Assert.True(await db.UserPrograms.AnyAsync(
            up => up.UserId == TestUserId && up.WorkoutProgramId == program.Id));
    }

    // Enrolling twice in the same program should not create a duplicate record
    [Fact]
    public async Task Enroll_Twice_DoesNotDuplicateUserProgram()
    {
        var db = CreateDb(nameof(Enroll_Twice_DoesNotDuplicateUserProgram));
        var program = new WorkoutProgram { Name = "Test", Description = "D", Level = Level.Beginner, Category = Category.Strength, DurationWeeks = 4, CreatedOn = DateTime.UtcNow };
        db.WorkoutPrograms.Add(program);
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        await controller.Enroll(program.Id);
        await controller.Enroll(program.Id);

        Assert.Equal(1, await db.UserPrograms.CountAsync(
            up => up.UserId == TestUserId && up.WorkoutProgramId == program.Id));
    }

    // Unenroll() should remove the UserProgram record from the database
    [Fact]
    public async Task Unenroll_WhenEnrolled_RemovesUserProgram()
    {
        var db = CreateDb(nameof(Unenroll_WhenEnrolled_RemovesUserProgram));
        var program = new WorkoutProgram { Name = "Test", Description = "D", Level = Level.Beginner, Category = Category.Strength, DurationWeeks = 4, CreatedOn = DateTime.UtcNow };
        db.WorkoutPrograms.Add(program);
        await db.SaveChangesAsync();

        db.UserPrograms.Add(new UserProgram { UserId = TestUserId, WorkoutProgramId = program.Id, StartDate = DateTime.UtcNow });
        await db.SaveChangesAsync();

        await CreateController(db).Unenroll(program.Id);

        Assert.False(await db.UserPrograms.AnyAsync(
            up => up.UserId == TestUserId && up.WorkoutProgramId == program.Id));
    }
}
