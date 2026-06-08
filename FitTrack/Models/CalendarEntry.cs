using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

// A scheduled workout slot; auto-generated for each plan day when a user enrols in a structured program.
public class CalendarEntry
{
    public int Id { get; set; }

    // ---- Foreign keys ----
    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    [Required]
    public int WorkoutProgramId { get; set; }

    [ForeignKey(nameof(WorkoutProgramId))]
    public WorkoutProgram WorkoutProgram { get; set; } = null!;

    // Null for programs without a WeeklyPlan (IsWorkoutOnly)
    public int? WeeklyPlanDayId { get; set; }

    [ForeignKey(nameof(WeeklyPlanDayId))]
    public WeeklyPlanDay? WeeklyPlanDay { get; set; }

    // ---- Schedule / state ----
    [Required]
    public DateTime ScheduledDate { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedOn { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
