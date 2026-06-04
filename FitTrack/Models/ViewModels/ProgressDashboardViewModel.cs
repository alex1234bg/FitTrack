namespace FitTrack.Models.ViewModels;

public class ProgressDashboardViewModel
{
    public float? StartingWeight  { get; set; }
    public float? LatestWeight    { get; set; }
    public float? WeightChange    => (StartingWeight.HasValue && LatestWeight.HasValue)
                                        ? LatestWeight - StartingWeight
                                        : null;

    public int TotalWorkouts      { get; set; }
    public int CurrentStreak      { get; set; }
    public int ActivePrograms     { get; set; }

    public List<WorkoutLog> RecentWorkouts  { get; set; } = new();
    public List<WeightLog>  WeightChartData { get; set; } = new(); // ascending, last 30
}
