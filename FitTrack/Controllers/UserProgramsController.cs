using FitTrack.Data;
using FitTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Controllers;

[Authorize]
public class UserProgramsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserProgramsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // POST: /UserPrograms/Enroll
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int workoutProgramId)
    {
        var userId = _userManager.GetUserId(User);

        var programExists = await _db.WorkoutPrograms.AnyAsync(p => p.Id == workoutProgramId);
        if (!programExists)
            return NotFound();

        var alreadyEnrolled = await _db.UserPrograms
            .AnyAsync(up => up.UserId == userId && up.WorkoutProgramId == workoutProgramId);

        if (!alreadyEnrolled)
        {
            _db.UserPrograms.Add(new UserProgram
            {
                UserId            = userId!,
                WorkoutProgramId  = workoutProgramId,
                StartDate         = DateTime.UtcNow,
                IsCompleted       = false,
                CompletedWorkouts = 0
            });

            // Auto-generate calendar entries from weekly plan
            var program = await _db.WorkoutPrograms
                .Include(p => p.WeeklyPlans.OrderBy(w => w.WeekNumber))
                    .ThenInclude(w => w.Days.OrderBy(d => d.DayNumber))
                .FirstOrDefaultAsync(p => p.Id == workoutProgramId);

            if (program is not null && program.WeeklyPlans.Any())
            {
                // Week 1 starts on the next Monday on or after today
                var today      = DateTime.Today;
                int daysToMon  = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
                var week1Start = today.AddDays(daysToMon == 0 ? 0 : daysToMon);

                var dayOffsets = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Monday"]    = 0, ["Tuesday"] = 1, ["Wednesday"] = 2,
                    ["Thursday"]  = 3, ["Friday"]  = 4, ["Saturday"]  = 5, ["Sunday"] = 6
                };

                var entries = new List<CalendarEntry>();
                foreach (var week in program.WeeklyPlans)
                {
                    var weekStart = week1Start.AddDays((week.WeekNumber - 1) * 7);
                    foreach (var day in week.Days)
                    {
                        if (!dayOffsets.TryGetValue(day.DayName, out int offset)) continue;
                        entries.Add(new CalendarEntry
                        {
                            UserId             = userId!,
                            WorkoutProgramId   = workoutProgramId,
                            WeeklyPlanDayId    = day.Id,
                            ScheduledDate      = weekStart.AddDays(offset),
                            IsCompleted        = false
                        });
                    }
                }

                _db.CalendarEntries.AddRange(entries);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "You have successfully enrolled in the program!";
        }
        else
        {
            TempData["Info"] = "You are already enrolled in this program.";
        }

        return RedirectToAction("Details", "WorkoutPrograms", new { id = workoutProgramId });
    }

    // POST: /UserPrograms/Unenroll
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unenroll(int workoutProgramId)
    {
        var userId = _userManager.GetUserId(User);

        var enrollment = await _db.UserPrograms
            .FirstOrDefaultAsync(up => up.UserId == userId && up.WorkoutProgramId == workoutProgramId);

        if (enrollment is not null)
        {
            _db.UserPrograms.Remove(enrollment);
            await _db.SaveChangesAsync();
            TempData["Success"] = "You have been unenrolled from the program.";
        }

        return RedirectToAction("Details", "WorkoutPrograms", new { id = workoutProgramId });
    }
}
