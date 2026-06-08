using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

// One structured week within a WorkoutProgram; contains ordered training days.
public class WeeklyPlan
{
    public int Id { get; set; }

    [Required]
    public int WorkoutProgramId { get; set; }

    [ForeignKey(nameof(WorkoutProgramId))]
    public WorkoutProgram WorkoutProgram { get; set; } = null!;

    [Required]
    [Range(1, 52)]
    public int WeekNumber { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    // ---- Navigation ----
    public ICollection<WeeklyPlanDay> Days { get; set; } = new List<WeeklyPlanDay>();
}
