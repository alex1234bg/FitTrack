namespace FitTrack.Models.ViewModels;

// Data bundle passed to the Progress/Index view; WeightChange is a computed property.
public class ProgressDashboardViewModel
{
    // ---- Weight ----
    public float? StartingWeight  { get; set; }
    public float? LatestWeight    { get; set; }
    public float? WeightChange    => (StartingWeight.HasValue && LatestWeight.HasValue)
                                        ? LatestWeight - StartingWeight
                                        : null;

    // ---- Activity ----
    public int TotalWorkouts      { get; set; }
    public int CurrentStreak      { get; set; }
    public int ActivePrograms     { get; set; }

    // ---- Lists ----
    public List<WorkoutLog> RecentWorkouts  { get; set; } = new();
    public List<WeightLog>  WeightChartData { get; set; } = new(); // ascending, last 30
}
