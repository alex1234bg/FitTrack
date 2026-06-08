using System.ComponentModel.DataAnnotations;

namespace FitTrack.Models;

// Form model for editing an existing exercise; ProgramId is carried through the round-trip for redirect.
public class ExerciseEditViewModel
{
    public int Id { get; set; }

    // Carried through the round-trip so we know where to redirect
    [Required]
    public int ProgramId { get; set; }

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
    public int Sets { get; set; }

    [Range(1, 300)]
    public int Reps { get; set; }

    public RepUnit RepUnit { get; set; } = RepUnit.Reps;

    [Url]
    [StringLength(500)]
    [Display(Name = "Image URL")]
    public string? ImageUrl { get; set; }
}
