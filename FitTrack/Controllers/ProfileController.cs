using FitTrack.Data;
using FitTrack.Models;
using FitTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    // =========================================================================
    // SETUP (onboarding)
    // =========================================================================

    // GET: /Profile/Setup
    [HttpGet]
    public async Task<IActionResult> Setup()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        if (user.HasCompletedProfile)
            return RedirectToAction(nameof(Index));

        return View(new UserProfileViewModel());
    }

    // POST: /Profile/Setup
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Setup(UserProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        user.Height              = model.Height;
        user.Weight              = model.Weight;
        user.Age                 = model.Age;
        user.Gender              = model.Gender;
        user.FitnessGoal         = model.FitnessGoal;
        user.FitnessLevel        = model.FitnessLevel;
        user.HasCompletedProfile = true;

        await _userManager.UpdateAsync(user);

        TempData["Success"] = "Welcome! Your profile is all set.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================================
    // INDEX (dashboard)
    // =========================================================================

    // GET: /Profile
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        if (!user.HasCompletedProfile)
            return RedirectToAction(nameof(Setup));

        // Enrolled programs
        var userPrograms = await _db.UserPrograms
            .Include(up => up.WorkoutProgram)
            .Where(up => up.UserId == user.Id)
            .OrderByDescending(up => up.StartDate)
            .ToListAsync();

        // Recommended programs — match by fitness level, prefer goal-matching category
        var matchingLevel = (Level)(int)user.FitnessLevel!.Value;

        var goalCategoryMap = new Dictionary<FitnessGoal, Category?>
        {
            { FitnessGoal.LoseWeight,          Category.Cardio      },
            { FitnessGoal.BuildMuscle,         Category.Strength    },
            { FitnessGoal.ImproveEndurance,    Category.Cardio      },
            { FitnessGoal.IncreaseFlexibility, Category.Flexibility },
            { FitnessGoal.StayHealthy,         null                 }
        };

        goalCategoryMap.TryGetValue(user.FitnessGoal!.Value, out var preferredCategory);

        var enrolledIds = userPrograms.Select(up => up.WorkoutProgramId).ToHashSet();

        var recommended = await _db.WorkoutPrograms
            .Where(p => p.Level == matchingLevel && !enrolledIds.Contains(p.Id))
            .OrderByDescending(p => preferredCategory.HasValue && p.Category == preferredCategory)
            .Take(3)
            .ToListAsync();

        // BMI
        double? bmi = null;
        string bmiCategory = "";
        string bmiClass = "bg-secondary";
        if (user.Height is > 0 && user.Weight is > 0)
        {
            var heightM = user.Height.Value / 100.0;
            bmi = Math.Round(user.Weight.Value / (heightM * heightM), 1);
            (bmiCategory, bmiClass) = bmi switch
            {
                < 18.5 => ("Underweight", "bg-info text-dark"),
                < 25.0 => ("Normal",      "bg-success"),
                < 30.0 => ("Overweight",  "bg-warning text-dark"),
                _      => ("Obese",       "bg-danger")
            };
        }

        ViewBag.UserPrograms    = userPrograms;
        ViewBag.Recommended     = recommended;
        ViewBag.Bmi             = bmi;
        ViewBag.BmiCategory     = bmiCategory;
        ViewBag.BmiClass        = bmiClass;

        return View(user);
    }

    // =========================================================================
    // EDIT
    // =========================================================================

    // GET: /Profile/Edit
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        var model = new UserProfileViewModel
        {
            Height      = user.Height      ?? 170,
            Weight      = user.Weight      ?? 70,
            Age         = user.Age         ?? 25,
            Gender      = user.Gender      ?? Gender.Other,
            FitnessGoal = user.FitnessGoal ?? FitnessGoal.StayHealthy,
            FitnessLevel = user.FitnessLevel ?? FitnessLevel.Beginner
        };

        return View(model);
    }

    // POST: /Profile/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        user.Height       = model.Height;
        user.Weight       = model.Weight;
        user.Age          = model.Age;
        user.Gender       = model.Gender;
        user.FitnessGoal  = model.FitnessGoal;
        user.FitnessLevel = model.FitnessLevel;

        await _userManager.UpdateAsync(user);

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
