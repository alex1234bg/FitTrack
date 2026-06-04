using FitTrack.Data;
using FitTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Controllers;

public class WorkoutProgramsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public WorkoutProgramsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // =========================================================================
    // PROGRAMS
    // =========================================================================

    // GET: /WorkoutPrograms
    public async Task<IActionResult> Index(Level? level, Category? category)
    {
        var query = _db.WorkoutPrograms
            .Include(p => p.ProgramReviews)
            .Include(p => p.WeeklyPlans)
            .AsQueryable();

        if (level.HasValue)
            query = query.Where(p => p.Level == level.Value);

        if (category.HasValue)
            query = query.Where(p => p.Category == category.Value);

        var programs = await query.OrderBy(p => p.Name).ToListAsync();

        ViewBag.SelectedLevel    = level;
        ViewBag.SelectedCategory = category;

        return View(programs);
    }

    // GET: /WorkoutPrograms/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var program = await _db.WorkoutPrograms
            .Include(p => p.ProgramExercises)
                .ThenInclude(pe => pe.Exercise)
            .Include(p => p.ProgramReviews)
                .ThenInclude(r => r.User)
            .Include(p => p.WeeklyPlans.OrderBy(w => w.WeekNumber))
                .ThenInclude(w => w.Days.OrderBy(d => d.DayNumber))
                    .ThenInclude(d => d.Exercises.OrderBy(e => e.OrderIndex))
            .FirstOrDefaultAsync(p => p.Id == id);

        if (program is null)
            return NotFound();

        var userId = _userManager.GetUserId(User);

        ViewBag.IsEnrolled = userId is not null &&
            await _db.UserPrograms.AnyAsync(up => up.UserId == userId && up.WorkoutProgramId == id);

        ViewBag.EnrolledCount = await _db.UserPrograms.CountAsync(up => up.WorkoutProgramId == id);

        ViewBag.UserHasReviewed = userId is not null &&
            program.ProgramReviews.Any(r => r.UserId == userId);

        ViewBag.AverageRating = program.ProgramReviews.Any()
            ? program.ProgramReviews.Average(r => r.Rating)
            : (double?)null;

        return View(program);
    }

    // =========================================================================
    // REVIEWS
    // =========================================================================

    // POST: /WorkoutPrograms/AddReview
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int workoutProgramId, int rating, string comment)
    {
        if (rating < 1 || rating > 5)
        {
            TempData["ReviewError"] = "Please select a rating between 1 and 5 stars.";
            return RedirectToAction(nameof(Details), new { id = workoutProgramId });
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            TempData["ReviewError"] = "Please write a comment.";
            return RedirectToAction(nameof(Details), new { id = workoutProgramId });
        }

        var userId = _userManager.GetUserId(User)!;

        var alreadyReviewed = await _db.ProgramReviews
            .AnyAsync(r => r.UserId == userId && r.WorkoutProgramId == workoutProgramId);

        if (alreadyReviewed)
        {
            TempData["ReviewError"] = "You have already reviewed this program.";
            return RedirectToAction(nameof(Details), new { id = workoutProgramId });
        }

        _db.ProgramReviews.Add(new ProgramReview
        {
            UserId           = userId,
            WorkoutProgramId = workoutProgramId,
            Rating           = rating,
            Comment          = comment.Trim(),
            CreatedOn        = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = "Your review has been posted. Thank you!";
        return RedirectToAction(nameof(Details), new { id = workoutProgramId });
    }

    // POST: /WorkoutPrograms/DeleteReview
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int reviewId, int workoutProgramId)
    {
        var userId = _userManager.GetUserId(User)!;
        var review = await _db.ProgramReviews.FindAsync(reviewId);

        if (review is null)
            return NotFound();

        if (review.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        _db.ProgramReviews.Remove(review);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Review deleted.";
        return RedirectToAction(nameof(Details), new { id = workoutProgramId });
    }
}
