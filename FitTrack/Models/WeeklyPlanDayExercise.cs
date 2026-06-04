using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

public class WeeklyPlanDayExercise
{
    public int Id { get; set; }

    [Required]
    public int WeeklyPlanDayId { get; set; }

    [ForeignKey(nameof(WeeklyPlanDayId))]
    public WeeklyPlanDay WeeklyPlanDay { get; set; } = null!;

    public int? ExerciseId { get; set; }

    [ForeignKey(nameof(ExerciseId))]
    public Exercise? Exercise { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string ExerciseName { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Sets { get; set; } = 3;

    [Range(1, 300)]
    public int Reps { get; set; } = 10;

    public RepUnit RepUnit { get; set; } = RepUnit.Reps;

    public int OrderIndex { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
