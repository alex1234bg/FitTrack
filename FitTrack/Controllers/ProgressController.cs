using FitTrack.Data;
using FitTrack.Models;
using FitTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Controllers;

[Authorize]
public class ProgressController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProgressController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: /Progress
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;

        var weightLogs = await _db.WeightLogs
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.LoggedOn)
            .ToListAsync();

        var workoutLogs = await _db.WorkoutLogs
            .Where(w => w.UserId == userId)
            .Include(w => w.WorkoutProgram)
            .OrderByDescending(w => w.LoggedOn)
            .ToListAsync();

        var activePrograms = await _db.UserPrograms
            .CountAsync(up => up.UserId == userId && !up.IsCompleted);

        var vm = new ProgressDashboardViewModel
        {
            StartingWeight  = weightLogs.FirstOrDefault()?.Weight,
            LatestWeight    = weightLogs.LastOrDefault()?.Weight,
            TotalWorkouts   = workoutLogs.Count,
            CurrentStreak   = CalculateStreak(workoutLogs.Select(w => w.LoggedOn).ToList()),
            ActivePrograms  = activePrograms,
            RecentWorkouts  = workoutLogs.Take(7).ToList(),
            WeightChartData = weightLogs.TakeLast(30).ToList()
        };

        return View(vm);
    }

    // POST: /Progress/LogWeight
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogWeight(float weight, DateTime loggedOn, string? notes)
    {
        if (weight < 20 || weight > 500)
        {
            TempData["Error"] = "Weight must be between 20 and 500 kg.";
            return RedirectToAction(nameof(Index));
        }

        var userId = _userManager.GetUserId(User)!;

        _db.WeightLogs.Add(new WeightLog
        {
            UserId    = userId,
            Weight    = weight,
            LoggedOn  = loggedOn == default ? DateTime.UtcNow : DateTime.SpecifyKind(loggedOn, DateTimeKind.Utc),
            Notes     = notes
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Weight {weight:F1} kg logged.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Progress/LogWorkout
    public async Task<IActionResult> LogWorkout()
    {
        var userId = _userManager.GetUserId(User)!;

        var enrolled = await _db.UserPrograms
            .Where(up => up.UserId == userId)
            .Include(up => up.WorkoutProgram)
            .OrderBy(up => up.WorkoutProgram.Name)
            .ToListAsync();

        var vm = new LogWorkoutViewModel
        {
            EnrolledPrograms = enrolled,
            LoggedOn         = DateTime.Today
        };

        return View(vm);
    }

    // POST: /Progress/LogWorkout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogWorkout(LogWorkoutViewModel model)
    {
        var userId = _userManager.GetUserId(User)!;


        model.EnrolledPrograms = await _db.UserPrograms
            .Where(up => up.UserId == userId)
            .Include(up => up.WorkoutProgram)
            .OrderBy(up => up.WorkoutProgram.Name)
            .ToListAsync();

        if (!ModelState.IsValid)
            return View(model);

        // Verify user is enrolled in that program
        var enrolled = model.EnrolledPrograms.Any(up => up.WorkoutProgramId == model.WorkoutProgramId);
        if (!enrolled)
        {
            ModelState.AddModelError("WorkoutProgramId", "You are not enrolled in this program.");
            return View(model);
        }

        _db.WorkoutLogs.Add(new WorkoutLog
        {
            UserId           = userId,
            WorkoutProgramId = model.WorkoutProgramId,
            LoggedOn         = DateTime.SpecifyKind(model.LoggedOn, DateTimeKind.Utc),
            DurationMinutes  = model.DurationMinutes,
            Feeling          = model.Feeling,
            Notes            = model.Notes
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = "Workout logged!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Progress/WeightHistory
    public async Task<IActionResult> WeightHistory()
    {
        var userId = _userManager.GetUserId(User)!;

        var logs = await _db.WeightLogs
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.LoggedOn)
            .ToListAsync();

        return View(logs);
    }

    // POST: /Progress/DeleteWeight
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWeight(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var log = await _db.WeightLogs.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId);

        if (log is not null)
        {
            _db.WeightLogs.Remove(log);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Weight entry deleted.";
        }

        return RedirectToAction(nameof(WeightHistory));
    }

    // GET: /Progress/WorkoutHistory
    public async Task<IActionResult> WorkoutHistory()
    {
        var userId = _userManager.GetUserId(User)!;

        var logs = await _db.WorkoutLogs
            .Where(w => w.UserId == userId)
            .Include(w => w.WorkoutProgram)
            .OrderByDescending(w => w.LoggedOn)
            .ToListAsync();

        return View(logs);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static int CalculateStreak(List<DateTime> logDates)
    {
        if (!logDates.Any()) return 0;

        var distinctDates = logDates
            .Select(d => d.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        var today    = DateTime.UtcNow.Date;
        var expected = distinctDates.Contains(today) ? today : today.AddDays(-1);
        var streak   = 0;

        foreach (var date in distinctDates)
        {
            if (date == expected)
            {
                streak++;
                expected = expected.AddDays(-1);
            }
            else if (date < expected)
                break;
        }

        return streak;
    }
}
