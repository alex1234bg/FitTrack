using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitTrack.Models;

public class WeeklyPlanDay
{
    public int Id { get; set; }

    [Required]
    public int WeeklyPlanId { get; set; }

    [ForeignKey(nameof(WeeklyPlanId))]
    public WeeklyPlan WeeklyPlan { get; set; } = null!;

    [Required]
    [Range(1, 7)]
    public int DayNumber { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string DayName { get; set; } = string.Empty;  

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Focus { get; set; } = string.Empty;    

    [StringLength(500)]
    public string? Notes { get; set; }

    public ICollection<WeeklyPlanDayExercise> Exercises { get; set; } = new List<WeeklyPlanDayExercise>();
}
