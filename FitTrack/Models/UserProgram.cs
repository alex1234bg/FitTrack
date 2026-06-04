using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

public class UserProgram
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

    [Required]
    public DateTime StartDate { get; set; }

    public bool IsCompleted { get; set; } = false;

    [Range(0, int.MaxValue)]
    public int CompletedWorkouts { get; set; } = 0;
}
