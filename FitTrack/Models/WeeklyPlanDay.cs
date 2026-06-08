using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

// A single training day within a WeeklyPlan (e.g. Week 1 — Monday, Upper Body).
public class WeeklyPlanDay
{
    public int Id { get; set; }

    [Required]
    public int WeeklyPlanId { get; set; }

    [ForeignKey(nameof(WeeklyPlanId))]
    public WeeklyPlan WeeklyPlan { get; set; } = null!;

    [Required]
    [Range(1, 7)]
    public int DayNumber { get; set; }

    // Full day name (e.g. "Monday") — used by UserProgramsController to calculate the calendar date
    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string DayName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Focus { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }

    // ---- Navigation ----
    public ICollection<WeeklyPlanDayExercise> Exercises { get; set; } = new List<WeeklyPlanDayExercise>();
}
