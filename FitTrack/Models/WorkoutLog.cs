using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

public enum Feeling
{
    Great,
    Good,
    Okay,
    Tough
}

// A completed workout session recorded by the user; tied to a specific program.
public class WorkoutLog
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
    public DateTime LoggedOn { get; set; } = DateTime.UtcNow;

    [Required]
    [Range(5, 300, ErrorMessage = "Duration must be between 5 and 300 minutes.")]
    public int DurationMinutes { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    public Feeling Feeling { get; set; }
}
