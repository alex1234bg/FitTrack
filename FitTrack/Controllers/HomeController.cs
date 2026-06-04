using System.Diagnostics;
using FitTrack.Data;
using FitTrack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    // GET: /
    public async Task<IActionResult> Index()
    {
        var programs = await _db.WorkoutPrograms
            .Include(p => p.ProgramReviews)
            .Include(p => p.WeeklyPlans)
            .ToListAsync();

        var featured = programs
            .OrderByDescending(p => p.ProgramReviews.Any()
                ? p.ProgramReviews.Average(r => r.Rating)
                : 0)
            .Take(3)
            .ToList();

        var totalPrograms    = programs.Count;
        var totalExercises   = await _db.Exercises.CountAsync();
        var totalMembers     = await _db.Users.CountAsync();
        var totalWorkoutLogs = await _db.WorkoutLogs.CountAsync();

        ViewBag.FeaturedPrograms  = featured;
        ViewBag.TotalPrograms     = totalPrograms;
        ViewBag.TotalExercises    = totalExercises;
        ViewBag.TotalMembers      = totalMembers;
        ViewBag.TotalWorkoutLogs  = totalWorkoutLogs;

        return View();
    }

    // GET: /Home/Privacy
    public IActionResult Privacy() => View();

    // GET: /Home/Error
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
