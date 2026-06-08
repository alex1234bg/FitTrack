using System.ComponentModel.DataAnnotations;

namespace FitTrack.Models.ViewModels;

// Form model for logging a completed workout session.
public class LogWorkoutViewModel
{
    // Populated in GET, used for the dropdown
    public List<UserProgram> EnrolledPrograms { get; set; } = new();

    [Required(ErrorMessage = "Please select a program.")]
    public int WorkoutProgramId { get; set; }

    [Required]
    public DateTime LoggedOn { get; set; } = DateTime.Today;

    [Required]
    [Range(5, 300, ErrorMessage = "Duration must be between 5 and 300 minutes.")]
    public int DurationMinutes { get; set; } = 45;

    [Required]
    public Feeling Feeling { get; set; } = Feeling.Good;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
