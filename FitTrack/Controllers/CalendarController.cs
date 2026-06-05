using FitTrack.Data;
using FitTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Controllers;

[Authorize]
public class CalendarController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CalendarController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: /Calendar or /Calendar/Index?year=2026&month=4
    public async Task<IActionResult> Index(int? year, int? month)
    {
        var now   = DateTime.Today;
        var y     = year  ?? now.Year;
        var m     = month ?? now.Month;

        if (m < 1) { m = 12; y--; }
        if (m > 12) { m = 1; y++; }

        var userId = _userManager.GetUserId(User)!;

        var firstDay  = new DateTime(y, m, 1);
        var lastDay   = firstDay.AddMonths(1).AddDays(-1);

        var entries = await _db.CalendarEntries
            .Include(e => e.WorkoutProgram)
            .Include(e => e.WeeklyPlanDay)
            .Where(e => e.UserId == userId
                     && e.ScheduledDate >= firstDay
                     && e.ScheduledDate <= lastDay)
            .OrderBy(e => e.ScheduledDate)
            .ToListAsync();

        ViewBag.Year       = y;
        ViewBag.Month      = m;
        ViewBag.MonthName  = firstDay.ToString("MMMM yyyy");
        ViewBag.DaysInMonth = DateTime.DaysInMonth(y, m);
        ViewBag.FirstDayOffset = ((int)firstDay.DayOfWeek + 6) % 7;
        ViewBag.Today      = now;
        ViewBag.AllPrograms = await _db.WorkoutPrograms
            .Where(p => p.IsWorkoutOnly)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return View(entries);
    }

    // POST: /Calendar/AddEntry
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEntry(int programId, int year, int month, int day)
    {
        var userId  = _userManager.GetUserId(User)!;
        var program = await _db.WorkoutPrograms.FindAsync(programId);
        if (program is null)
            return Json(new { success = false, message = "Program not found." });

        var entry = new CalendarEntry
        {
            UserId           = userId,
            WorkoutProgramId = programId,
            ScheduledDate    = new DateTime(year, month, day)
        };
        _db.CalendarEntries.Add(entry);
        await _db.SaveChangesAsync();

        return Json(new { success = true, entryId = entry.Id, programName = program.Name });
    }

    // POST: /Calendar/RemoveEntry
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveEntry(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var entry  = await _db.CalendarEntries.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (entry is null)
            return Json(new { success = false, message = "Entry not found." });

        _db.CalendarEntries.Remove(entry);
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST: /Calendar/ToggleComplete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleComplete(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var entry  = await _db.CalendarEntries.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (entry is null)
            return Json(new { success = false, message = "Entry not found." });

        entry.IsCompleted = !entry.IsCompleted;
        entry.CompletedOn = entry.IsCompleted ? DateTime.UtcNow : null;
        await _db.SaveChangesAsync();

        return Json(new { success = true, isCompleted = entry.IsCompleted, entryId = entry.Id });
    }

    // POST: /Calendar/AddNote
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNote(int id, string? notes)
    {
        var userId = _userManager.GetUserId(User)!;
        var entry  = await _db.CalendarEntries.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (entry is null)
            return Json(new { success = false, message = "Entry not found." });

        entry.Notes = notes?.Trim();
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // GET: /Calendar/GetDayEntries?year=2026&month=4&day=22
    public async Task<IActionResult> GetDayEntries(int year, int month, int day)
    {
        var userId = _userManager.GetUserId(User)!;
        var date   = new DateTime(year, month, day);

        var entries = await _db.CalendarEntries
            .Include(e => e.WorkoutProgram)
            .Include(e => e.WeeklyPlanDay)
                .ThenInclude(d => d!.WeeklyPlan)
            .Include(e => e.WeeklyPlanDay)
                .ThenInclude(d => d!.Exercises.OrderBy(ex => ex.OrderIndex))
            .Where(e => e.UserId == userId && e.ScheduledDate.Date == date.Date)
            .OrderBy(e => e.Id)
            .ToListAsync();

        var result = entries.Select(e => new
        {
            id          = e.Id,
            programName = e.WorkoutProgram.Name,
            weekNumber  = e.WeeklyPlanDay?.WeeklyPlan?.WeekNumber,
            dayName     = e.WeeklyPlanDay?.DayName,
            dayFocus    = e.WeeklyPlanDay?.Focus,
            isCompleted = e.IsCompleted,
            notes       = e.Notes,
            exercises   = e.WeeklyPlanDay?.Exercises.Select(ex => new
            {
                name  = ex.ExerciseName,
                sets  = ex.Sets,
                reps  = ex.Reps,
                unit  = ex.RepUnit.ToString()
            }) ?? Enumerable.Empty<object>()
        });

        return Json(result);
    }
}
