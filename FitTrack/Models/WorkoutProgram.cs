using System.ComponentModel.DataAnnotations;

namespace FitTrack.Models;

public enum Level
{
    Beginner,
    Intermediate,
    Advanced
}

public enum Category
{
    Strength,
    Cardio,
    Flexibility
}

// Admin-created fitness program; users enrol via UserProgram, structured weeks via WeeklyPlan.
public class WorkoutProgram
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Level Level { get; set; }

    [Required]
    public Category Category { get; set; }

    [Range(1, 52)]
    public int DurationWeeks { get; set; }

    [Url]
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    // When true the program uses free-form exercises only (no WeeklyPlan)
    public bool IsWorkoutOnly { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    // ---- Navigation ----
    public ICollection<ProgramExercise> ProgramExercises { get; set; } = new List<ProgramExercise>();

    public ICollection<UserProgram> UserPrograms { get; set; } = new List<UserProgram>();

    public ICollection<ProgramReview> ProgramReviews { get; set; } = new List<ProgramReview>();

    public ICollection<WeeklyPlan> WeeklyPlans { get; set; } = new List<WeeklyPlan>();
}
