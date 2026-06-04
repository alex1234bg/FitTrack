using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

public class CalendarEntry
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    [Required]
    public int WorkoutProgramId { get; set; }

    [ForeignKey(nameof(WorkoutProgramId))]
    public WorkoutProgram WorkoutProgram { get; set; } = null!;

    public int? WeeklyPlanDayId { get; set; }

    [ForeignKey(nameof(WeeklyPlanDayId))]
    public WeeklyPlanDay? WeeklyPlanDay { get; set; }

    [Required]
    public DateTime ScheduledDate { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedOn { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
