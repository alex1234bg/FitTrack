using FitTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager);
        await SeedWorkoutProgramsAsync(db);
    }

    // -------------------------------------------------------------------------
    // Roles
    // -------------------------------------------------------------------------
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = ["Admin", "User"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // -------------------------------------------------------------------------
    // Admin user
    // -------------------------------------------------------------------------
    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail    = "admin@fittrack.com";
        const string adminPassword = "Admin123!";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName       = adminEmail,
            Email          = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }

    // -------------------------------------------------------------------------
    // Workout programs + exercises
    // -------------------------------------------------------------------------
    private static async Task SeedWorkoutProgramsAsync(ApplicationDbContext db)
    {
        if (await db.WorkoutPrograms.AnyAsync())
            return;

        var programs = new List<WorkoutProgram>
        {
            new()
            {
                Name          = "Beginner Full Body",
                Description   = "A four-week full-body strength program designed for those just starting their fitness journey. Focus on learning proper form with compound movements.",
                Level         = Level.Beginner,
                Category      = Category.Strength,
                DurationWeeks = 4,
                CreatedOn     = DateTime.UtcNow
            },
            new()
            {
                Name          = "Cardio Blast",
                Description   = "A six-week intermediate cardio program that combines HIIT and steady-state cardio to maximise fat burning and improve cardiovascular endurance.",
                Level         = Level.Intermediate,
                Category      = Category.Cardio,
                DurationWeeks = 6,
                CreatedOn     = DateTime.UtcNow
            },
            new()
            {
                Name          = "Advanced Power",
                Description   = "An eight-week advanced strength program built around heavy compound lifts and progressive overload for experienced athletes looking to build serious power.",
                Level         = Level.Advanced,
                Category      = Category.Strength,
                DurationWeeks = 8,
                CreatedOn     = DateTime.UtcNow
            }
        };

        await db.WorkoutPrograms.AddRangeAsync(programs);
        await db.SaveChangesAsync();

        var exercises = new List<Exercise>
        {
            // --- Beginner Full Body ---
            new() { Name = "Bodyweight Squat",   Description = "Stand feet shoulder-width apart, lower until thighs are parallel to the floor, then return to standing.",   MuscleGroup = "Quadriceps",  Sets = 3, Reps = 12 },
            new() { Name = "Push-Up",             Description = "Keep body straight from head to heels, lower chest to the floor and press back up.",                        MuscleGroup = "Chest",       Sets = 3, Reps = 10 },
            new() { Name = "Dumbbell Row",        Description = "Hinge at the hips, pull a dumbbell from a hanging position to your hip, keeping the elbow close.",          MuscleGroup = "Back",        Sets = 3, Reps = 10 },

            // --- Cardio Blast ---
            new() { Name = "Jumping Jacks",       Description = "Jump feet wide while raising arms overhead, then return to starting position in a continuous rhythm.",       MuscleGroup = "Full Body",   Sets = 4, Reps = 30 },
            new() { Name = "High Knees",          Description = "Run in place driving knees up to hip height as fast as possible while pumping arms.",                       MuscleGroup = "Core / Legs", Sets = 4, Reps = 40 },
            new() { Name = "Burpee",              Description = "From standing, drop to a push-up, perform the push-up, jump feet in and explode upward with arms overhead.", MuscleGroup = "Full Body",   Sets = 3, Reps = 15 },

            // --- Advanced Power ---
            new() { Name = "Barbell Back Squat",  Description = "Bar rests across upper traps; descend until hips are below parallel, drive through heels to stand.",        MuscleGroup = "Quadriceps",  Sets = 5, Reps = 5  },
            new() { Name = "Deadlift",            Description = "Grip bar at hip width, keep back neutral, drive hips forward to lift bar from the floor to lockout.",       MuscleGroup = "Hamstrings",  Sets = 5, Reps = 3  },
            new() { Name = "Bench Press",         Description = "Lower the bar under control to mid-chest, press to full elbow extension while keeping feet flat on floor.", MuscleGroup = "Chest",       Sets = 5, Reps = 5  }
        };

        await db.Exercises.AddRangeAsync(exercises);
        await db.SaveChangesAsync();

        // Link exercises to programs via ProgramExercise
        var programExercises = new List<ProgramExercise>
        {
            // Beginner Full Body (index 0) — exercises 0-2
            new() { WorkoutProgramId = programs[0].Id, ExerciseId = exercises[0].Id },
            new() { WorkoutProgramId = programs[0].Id, ExerciseId = exercises[1].Id },
            new() { WorkoutProgramId = programs[0].Id, ExerciseId = exercises[2].Id },

            // Cardio Blast (index 1) — exercises 3-5
            new() { WorkoutProgramId = programs[1].Id, ExerciseId = exercises[3].Id },
            new() { WorkoutProgramId = programs[1].Id, ExerciseId = exercises[4].Id },
            new() { WorkoutProgramId = programs[1].Id, ExerciseId = exercises[5].Id },

            // Advanced Power (index 2) — exercises 6-8
            new() { WorkoutProgramId = programs[2].Id, ExerciseId = exercises[6].Id },
            new() { WorkoutProgramId = programs[2].Id, ExerciseId = exercises[7].Id },
            new() { WorkoutProgramId = programs[2].Id, ExerciseId = exercises[8].Id }
        };

        await db.ProgramExercises.AddRangeAsync(programExercises);
        await db.SaveChangesAsync();
    }
}
