using System.ComponentModel.DataAnnotations;

namespace FitTrack.Models;

public enum RepUnit
{
    Reps,
    Seconds,
    Minutes
}

public class Exercise
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string MuscleGroup { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Sets { get; set; }

    [Range(1, 300)]
    public int Reps { get; set; }

    public RepUnit RepUnit { get; set; } = RepUnit.Reps;

    [Url]
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public ICollection<ProgramExercise> ProgramExercises { get; set; } = new List<ProgramExercise>();
}
