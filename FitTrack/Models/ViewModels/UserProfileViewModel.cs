using System.ComponentModel.DataAnnotations;

namespace FitTrack.Models.ViewModels;

// Form model for the onboarding (Setup) and profile-edit pages.
public class UserProfileViewModel
{
    [Required]
    [Range(100, 250, ErrorMessage = "Height must be between 100 and 250 cm.")]
    [Display(Name = "Height (cm)")]
    public float Height { get; set; }

    [Required]
    [Range(30, 300, ErrorMessage = "Weight must be between 30 and 300 kg.")]
    [Display(Name = "Weight (kg)")]
    public float Weight { get; set; }

    [Required]
    [Range(10, 100, ErrorMessage = "Age must be between 10 and 100.")]
    public int Age { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [Required]
    [Display(Name = "Fitness Goal")]
    public FitnessGoal FitnessGoal { get; set; }

    [Required]
    [Display(Name = "Fitness Level")]
    public FitnessLevel FitnessLevel { get; set; }
}
