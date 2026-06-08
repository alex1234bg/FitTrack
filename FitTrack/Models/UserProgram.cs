using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

// Enrolment record — one row per user/program pair; tracks start date and completion state.
public class UserProgram
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

    // ---- Enrolment state ----
    [Required]
    public DateTime StartDate { get; set; }

    public bool IsCompleted { get; set; } = false;

    [Range(0, int.MaxValue)]
    public int CompletedWorkouts { get; set; } = 0;
}
