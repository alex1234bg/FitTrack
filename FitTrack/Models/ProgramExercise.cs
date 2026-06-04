using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

public class ProgramExercise
{
    public int Id { get; set; }

    [Required]
    public int WorkoutProgramId { get; set; }

    [ForeignKey(nameof(WorkoutProgramId))]
    public WorkoutProgram WorkoutProgram { get; set; } = null!;

    [Required]
    public int ExerciseId { get; set; }

    [ForeignKey(nameof(ExerciseId))]
    public Exercise Exercise { get; set; } = null!;
}
