using Microsoft.AspNetCore.Identity;

namespace FitTrack.Models;

public enum Gender
{
    Male,
    Female,
    Other
}

public enum FitnessGoal
{
    LoseWeight,
    BuildMuscle,
    ImproveEndurance,
    IncreaseFlexibility,
    StayHealthy
}

public enum FitnessLevel
{
    Beginner,
    Intermediate,
    Advanced
}

// Extends ASP.NET Identity user with a fitness profile; profile fields are optional until onboarding completes.
public class ApplicationUser : IdentityUser
{
    // Profile fields — nullable so existing rows don't break on migration
    public float?       Height              { get; set; }   // cm
    public float?       Weight              { get; set; }   // kg
    public int?         Age                 { get; set; }
    public Gender?      Gender              { get; set; }
    public FitnessGoal? FitnessGoal         { get; set; }
    public FitnessLevel? FitnessLevel       { get; set; }
    public bool         HasCompletedProfile { get; set; } = false;

    // ---- Navigation ----
    public ICollection<UserProgram> UserPrograms { get; set; } = new List<UserProgram>();
}
