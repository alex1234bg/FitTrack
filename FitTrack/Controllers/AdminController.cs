using FitTrack.Data;
using FitTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // =========================================================================
    // PROGRAMS
    // =========================================================================

    // GET: /Admin
    public async Task<IActionResult> Index()
    {
        var programs = await _db.WorkoutPrograms
            .Include(p => p.ProgramExercises)
            .Include(p => p.UserPrograms)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return View(programs);
    }

    // GET: /Admin/Create
    public IActionResult Create() => View(new WorkoutProgram());

    // POST: /Admin/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkoutProgram model)
    {
        if (!ModelState.IsValid)
            return View(model);

        model.CreatedOn = DateTime.UtcNow;
        _db.WorkoutPrograms.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Program \"{model.Name}\" created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var program = await _db.WorkoutPrograms.FindAsync(id);
        if (program is null) return NotFound();
        return View(program);
    }

    // POST: /Admin/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WorkoutProgram model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var existing = await _db.WorkoutPrograms.FindAsync(id);
        if (existing is null) return NotFound();

        existing.Name          = model.Name;
        existing.Description   = model.Description;
        existing.Level         = model.Level;
        existing.Category      = model.Category;
        existing.DurationWeeks = model.DurationWeeks;
        existing.ImageUrl      = model.ImageUrl;
        existing.IsWorkoutOnly = model.IsWorkoutOnly;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Program \"{existing.Name}\" updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Admin/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var program = await _db.WorkoutPrograms
            .Include(p => p.UserPrograms)
            .Include(p => p.ProgramExercises)
            .Include(p => p.ProgramReviews)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (program is null) return NotFound();

        ViewBag.WorkoutLogCount = await _db.WorkoutLogs.CountAsync(w => w.WorkoutProgramId == id);

        return View(program);
    }

    // POST: /Admin/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var program = await _db.WorkoutPrograms.FindAsync(id);
        if (program is null) return NotFound();

        try
        {
            // 1. WorkoutLogs (Restrict FK — must be removed manually first)
            var workoutLogs = await _db.WorkoutLogs
                .Where(w => w.WorkoutProgramId == id).ToListAsync();
            _db.WorkoutLogs.RemoveRange(workoutLogs);

            // 2. UserPrograms
            var userPrograms = await _db.UserPrograms
                .Where(up => up.WorkoutProgramId == id).ToListAsync();
            _db.UserPrograms.RemoveRange(userPrograms);

            // 3. ProgramReviews
            var reviews = await _db.ProgramReviews
                .Where(r => r.WorkoutProgramId == id).ToListAsync();
            _db.ProgramReviews.RemoveRange(reviews);

            // 4. ProgramExercises
            var programExercises = await _db.ProgramExercises
                .Where(pe => pe.WorkoutProgramId == id).ToListAsync();
            _db.ProgramExercises.RemoveRange(programExercises);

            // 5. The program itself
            _db.WorkoutPrograms.Remove(program);

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Program \"{program.Name}\" and all related data deleted.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Could not delete \"{program.Name}\": {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // EXERCISES
    // =========================================================================

    // GET: /Admin/ManageExercises/5
    public async Task<IActionResult> ManageExercises(int id)
    {
        var program = await _db.WorkoutPrograms
            .Include(p => p.ProgramExercises)
                .ThenInclude(pe => pe.Exercise)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (program is null) return NotFound();


        ViewBag.NewExercise = new ExerciseCreateViewModel { WorkoutProgramId = id };
        return View(program);
    }

    // POST: /Admin/ExerciseCreate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExerciseCreate(ExerciseCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {

            var prog = await _db.WorkoutPrograms
                .Include(p => p.ProgramExercises).ThenInclude(pe => pe.Exercise)
                .FirstOrDefaultAsync(p => p.Id == model.WorkoutProgramId);

            if (prog is null) return NotFound();
            ViewBag.NewExercise = model;
            TempData["CreateError"] = true;
            return View("ManageExercises", prog);
        }

        var exercise = new Exercise
        {
            Name        = model.Name,
            Description = model.Description,
            MuscleGroup = model.MuscleGroup,
            Sets        = model.Sets,
            Reps        = model.Reps,
            RepUnit     = model.RepUnit,
            ImageUrl    = model.ImageUrl
        };

        _db.Exercises.Add(exercise);
        await _db.SaveChangesAsync();

        _db.ProgramExercises.Add(new ProgramExercise
        {
            WorkoutProgramId = model.WorkoutProgramId,
            ExerciseId       = exercise.Id
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Exercise \"{exercise.Name}\" added.";
        return RedirectToAction(nameof(ManageExercises), new { id = model.WorkoutProgramId });
    }

    // GET: /Admin/ExerciseEdit/5?programId=3
    public async Task<IActionResult> ExerciseEdit(int id, int programId)
    {
        var exercise = await _db.Exercises.FindAsync(id);
        if (exercise is null) return NotFound();

        var vm = new ExerciseEditViewModel
        {
            Id          = exercise.Id,
            ProgramId   = programId,
            Name        = exercise.Name,
            Description = exercise.Description,
            MuscleGroup = exercise.MuscleGroup,
            Sets        = exercise.Sets,
            Reps        = exercise.Reps,
            RepUnit     = exercise.RepUnit,
            ImageUrl    = exercise.ImageUrl
        };

        return View(vm);
    }

    // POST: /Admin/ExerciseEdit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExerciseEdit(ExerciseEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var exercise = await _db.Exercises.FindAsync(model.Id);
        if (exercise is null) return NotFound();

        exercise.Name        = model.Name;
        exercise.Description = model.Description;
        exercise.MuscleGroup = model.MuscleGroup;
        exercise.Sets        = model.Sets;
        exercise.Reps        = model.Reps;
        exercise.RepUnit     = model.RepUnit;
        exercise.ImageUrl    = model.ImageUrl;

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Exercise \"{exercise.Name}\" updated.";
        return RedirectToAction(nameof(ManageExercises), new { id = model.ProgramId });
    }

    // POST: /Admin/ExerciseDelete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExerciseDelete(int exerciseId, int programId)
    {
        var link = await _db.ProgramExercises
            .FirstOrDefaultAsync(pe => pe.ExerciseId == exerciseId && pe.WorkoutProgramId == programId);

        if (link is not null)
            _db.ProgramExercises.Remove(link);

        var exercise = await _db.Exercises.FindAsync(exerciseId);
        if (exercise is not null)
            _db.Exercises.Remove(exercise);

        await _db.SaveChangesAsync();

        TempData["Success"] = "Exercise deleted.";
        return RedirectToAction(nameof(ManageExercises), new { id = programId });
    }

    // =========================================================================
    // WEEKLY PLANS
    // =========================================================================

    // GET: /Admin/ManageWeeklyPlans/5  (id = programId)
    public async Task<IActionResult> ManageWeeklyPlans(int id)
    {
        var program = await _db.WorkoutPrograms
            .Include(p => p.WeeklyPlans)
                .ThenInclude(w => w.Days)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (program is null) return NotFound();
        return View(program);
    }

    // POST: /Admin/CreateWeek
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWeek(int workoutProgramId, int weekNumber,
                                                string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Json(new { success = false, message = "Title is required." });

        var week = new WeeklyPlan
        {
            WorkoutProgramId = workoutProgramId,
            WeekNumber       = weekNumber,
            Title            = title.Trim(),
            Description      = description?.Trim()
        };
        _db.WeeklyPlans.Add(week);
        await _db.SaveChangesAsync();

        return Json(new { success = true, id = week.Id, weekNumber = week.WeekNumber,
                          title = week.Title, description = week.Description });
    }

    // GET: /Admin/EditWeek/3
    public async Task<IActionResult> EditWeek(int id)
    {
        var week = await _db.WeeklyPlans.FindAsync(id);
        if (week is null) return NotFound();
        return View(week);
    }

    // POST: /Admin/EditWeek
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditWeek(int id, int weekNumber, string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Json(new { success = false, message = "Title is required." });

        var existing = await _db.WeeklyPlans.FindAsync(id);
        if (existing is null)
            return Json(new { success = false, message = "Week not found." });

        existing.WeekNumber  = weekNumber;
        existing.Title       = title.Trim();
        existing.Description = description?.Trim();
        await _db.SaveChangesAsync();

        return Json(new { success = true, weekNumber = existing.WeekNumber,
                          title = existing.Title, description = existing.Description });
    }

    // POST: /Admin/DeleteWeek
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWeek(int id)
    {
        var week = await _db.WeeklyPlans.FindAsync(id);
        if (week is null)
            return Json(new { success = false, message = "Week not found." });

        _db.WeeklyPlans.Remove(week);
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // GET: /Admin/ManageWeekDays/3  (id = weekId)
    public async Task<IActionResult> ManageWeekDays(int id)
    {
        var week = await _db.WeeklyPlans
            .Include(w => w.WorkoutProgram)
            .Include(w => w.Days.OrderBy(d => d.DayNumber))
                .ThenInclude(d => d.Exercises.OrderBy(e => e.OrderIndex))
            .FirstOrDefaultAsync(w => w.Id == id);

        if (week is null) return NotFound();
        return View(week);
    }

    // POST: /Admin/CreateDay
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDay(int weeklyPlanId, int dayNumber,
                                               string dayName, string focus, string? notes)
    {
        if (string.IsNullOrWhiteSpace(dayName) || string.IsNullOrWhiteSpace(focus))
            return Json(new { success = false, message = "Day name and focus are required." });

        var day = new WeeklyPlanDay
        {
            WeeklyPlanId = weeklyPlanId,
            DayNumber    = dayNumber,
            DayName      = dayName.Trim(),
            Focus        = focus.Trim(),
            Notes        = notes?.Trim()
        };
        _db.WeeklyPlanDays.Add(day);
        await _db.SaveChangesAsync();

        return Json(new { success = true, id = day.Id, dayNumber = day.DayNumber,
                          dayName = day.DayName, focus = day.Focus, notes = day.Notes });
    }

    // GET: /Admin/EditDay/3
    public async Task<IActionResult> EditDay(int id)
    {
        var day = await _db.WeeklyPlanDays.FindAsync(id);
        if (day is null) return NotFound();
        return View(day);
    }

    // POST: /Admin/EditDay
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDay(int id, int dayNumber, string dayName, string focus, string? notes)
    {
        if (string.IsNullOrWhiteSpace(dayName) || string.IsNullOrWhiteSpace(focus))
            return Json(new { success = false, message = "Day name and focus are required." });

        var existing = await _db.WeeklyPlanDays.FindAsync(id);
        if (existing is null)
            return Json(new { success = false, message = "Day not found." });

        existing.DayNumber = dayNumber;
        existing.DayName   = dayName.Trim();
        existing.Focus     = focus.Trim();
        existing.Notes     = notes?.Trim();
        await _db.SaveChangesAsync();

        return Json(new { success = true, dayNumber = existing.DayNumber,
                          dayName = existing.DayName, focus = existing.Focus, notes = existing.Notes });
    }

    // POST: /Admin/DeleteDay
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDay(int id)
    {
        var day = await _db.WeeklyPlanDays.FindAsync(id);
        if (day is null)
            return Json(new { success = false, message = "Day not found." });

        _db.WeeklyPlanDays.Remove(day);
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST: /Admin/EditDayExercise
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDayExercise(int id, string exerciseName,
                                                     int sets, int reps, RepUnit repUnit, string? notes)
    {
        if (string.IsNullOrWhiteSpace(exerciseName))
            return Json(new { success = false, message = "Exercise name is required." });

        var entry = await _db.WeeklyPlanDayExercises.FindAsync(id);
        if (entry is null)
            return Json(new { success = false, message = "Exercise not found." });

        entry.ExerciseName = exerciseName.Trim();
        entry.Sets         = Math.Clamp(sets, 1, 20);
        entry.Reps         = Math.Clamp(reps, 1, 300);
        entry.RepUnit      = repUnit;
        entry.Notes        = notes?.Trim();
        await _db.SaveChangesAsync();

        return Json(new { success = true, exerciseName = entry.ExerciseName, sets = entry.Sets,
                          reps = entry.Reps, repUnit = (int)entry.RepUnit, notes = entry.Notes });
    }

    // POST: /Admin/AddDayExercise
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDayExercise(int dayId, string exerciseName,
                                                    int sets, int reps, RepUnit repUnit, string? notes)
    {
        if (string.IsNullOrWhiteSpace(exerciseName))
            return Json(new { success = false, message = "Exercise name is required." });

        var day = await _db.WeeklyPlanDays.FindAsync(dayId);
        if (day is null)
            return Json(new { success = false, message = "Day not found." });

        var maxOrder = await _db.WeeklyPlanDayExercises
            .Where(e => e.WeeklyPlanDayId == dayId)
            .Select(e => (int?)e.OrderIndex)
            .MaxAsync() ?? 0;

        var entry = new WeeklyPlanDayExercise
        {
            WeeklyPlanDayId = dayId,
            ExerciseName    = exerciseName.Trim(),
            Sets            = Math.Clamp(sets, 1, 20),
            Reps            = Math.Clamp(reps, 1, 300),
            RepUnit         = repUnit,
            OrderIndex      = maxOrder + 1,
            Notes           = notes?.Trim()
        };

        _db.WeeklyPlanDayExercises.Add(entry);
        await _db.SaveChangesAsync();

        return Json(new
        {
            success  = true,
            id       = entry.Id,
            name     = entry.ExerciseName,
            sets     = entry.Sets,
            reps     = entry.Reps,
            repUnit  = (int)entry.RepUnit,
            notes    = entry.Notes,
            order    = entry.OrderIndex
        });
    }

    // POST: /Admin/RemoveDayExercise
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveDayExercise(int id)
    {
        var entry = await _db.WeeklyPlanDayExercises.FindAsync(id);
        if (entry is null)
            return Json(new { success = false, message = "Exercise not found." });

        _db.WeeklyPlanDayExercises.Remove(entry);
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    // =========================================================================
    // REVIEWS
    // =========================================================================

    // GET: /Admin/Reviews
    public async Task<IActionResult> Reviews()
    {
        var reviews = await _db.ProgramReviews
            .Include(r => r.User)
            .Include(r => r.WorkoutProgram)
            .OrderByDescending(r => r.CreatedOn)
            .ToListAsync();

        return View(reviews);
    }

    // POST: /Admin/DeleteReview
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await _db.ProgramReviews.FindAsync(id);
        if (review is not null)
        {
            _db.ProgramReviews.Remove(review);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Review deleted.";
        }

        return RedirectToAction(nameof(Reviews));
    }

    // =========================================================================
    // USERS
    // =========================================================================

    // GET: /Admin/Users
    public async Task<IActionResult> Users()
    {
        var users = await _db.Users
            .Include(u => u.UserPrograms)
            .OrderBy(u => u.Email)
            .ToListAsync();

        var userRoles = new Dictionary<string, IList<string>>();
        foreach (var user in users)
            userRoles[user.Id] = await _userManager.GetRolesAsync(user);

        ViewBag.UserRoles = userRoles;
        return View(users);
    }
}
