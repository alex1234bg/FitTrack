using System.ComponentModel.DataAnnotations;

namespace FitTrack.Models;

public class ExerciseCreateViewModel
{
    [Required]
    public int WorkoutProgramId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Muscle Group")]
    public string MuscleGroup { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Sets { get; set; } = 3;

    [Range(1, 300)]
    public int Reps { get; set; } = 10;

    public RepUnit RepUnit { get; set; } = RepUnit.Reps;

    [Url]
    [StringLength(500)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }
}
