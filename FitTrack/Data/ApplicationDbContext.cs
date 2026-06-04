using FitTrack.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitTrack.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkoutProgram>       WorkoutPrograms        { get; set; }
    public DbSet<Exercise>             Exercises              { get; set; }
    public DbSet<ProgramExercise>      ProgramExercises       { get; set; }
    public DbSet<UserProgram>          UserPrograms           { get; set; }
    public DbSet<WeightLog>            WeightLogs             { get; set; }
    public DbSet<WorkoutLog>           WorkoutLogs            { get; set; }
    public DbSet<ProgramReview>        ProgramReviews         { get; set; }
    public DbSet<WeeklyPlan>           WeeklyPlans            { get; set; }
    public DbSet<WeeklyPlanDay>        WeeklyPlanDays         { get; set; }
    public DbSet<WeeklyPlanDayExercise> WeeklyPlanDayExercises { get; set; }
    public DbSet<CalendarEntry>        CalendarEntries        { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // WorkoutProgram <-> Exercise (many-to-many via ProgramExercise)
        modelBuilder.Entity<ProgramExercise>(entity =>
        {
            entity.HasKey(pe => pe.Id);

            entity.HasOne(pe => pe.WorkoutProgram)
                  .WithMany(wp => wp.ProgramExercises)
                  .HasForeignKey(pe => pe.WorkoutProgramId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pe => pe.Exercise)
                  .WithMany(e => e.ProgramExercises)
                  .HasForeignKey(pe => pe.ExerciseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ApplicationUser <-> WorkoutProgram (many-to-many via UserProgram)
        modelBuilder.Entity<UserProgram>(entity =>
        {
            entity.HasKey(up => up.Id);

            entity.HasOne(up => up.User)
                  .WithMany(u => u.UserPrograms)
                  .HasForeignKey(up => up.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(up => up.WorkoutProgram)
                  .WithMany(wp => wp.UserPrograms)
                  .HasForeignKey(up => up.WorkoutProgramId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // WeightLog -> ApplicationUser
        modelBuilder.Entity<WeightLog>(entity =>
        {
            entity.HasOne(wl => wl.User)
                  .WithMany()
                  .HasForeignKey(wl => wl.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkoutLog -> ApplicationUser (cascade) + WorkoutProgram (restrict — manual delete required)
        modelBuilder.Entity<WorkoutLog>(entity =>
        {
            entity.HasOne(wl => wl.User)
                  .WithMany()
                  .HasForeignKey(wl => wl.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(wl => wl.WorkoutProgram)
                  .WithMany()
                  .HasForeignKey(wl => wl.WorkoutProgramId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ProgramReview: unique per user+program
        modelBuilder.Entity<ProgramReview>(entity =>
        {
            entity.HasIndex(r => new { r.UserId, r.WorkoutProgramId })
                  .IsUnique();

            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.WorkoutProgram)
                  .WithMany(wp => wp.ProgramReviews)
                  .HasForeignKey(r => r.WorkoutProgramId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // WeeklyPlan -> WorkoutProgram (cascade: deleting program removes all its plans)
        modelBuilder.Entity<WeeklyPlan>(entity =>
        {
            entity.HasOne(w => w.WorkoutProgram)
                  .WithMany(p => p.WeeklyPlans)
                  .HasForeignKey(w => w.WorkoutProgramId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // WeeklyPlanDay -> WeeklyPlan (cascade)
        modelBuilder.Entity<WeeklyPlanDay>(entity =>
        {
            entity.HasOne(d => d.WeeklyPlan)
                  .WithMany(w => w.Days)
                  .HasForeignKey(d => d.WeeklyPlanId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // CalendarEntry -> User (cascade) + WorkoutProgram (restrict) + WeeklyPlanDay (restrict)
        modelBuilder.Entity<CalendarEntry>(entity =>
        {
            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.WorkoutProgram)
                  .WithMany()
                  .HasForeignKey(c => c.WorkoutProgramId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.WeeklyPlanDay)
                  .WithMany()
                  .HasForeignKey(c => c.WeeklyPlanDayId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // WeeklyPlanDayExercise -> WeeklyPlanDay (cascade) + Exercise (restrict)
        modelBuilder.Entity<WeeklyPlanDayExercise>(entity =>
        {
            entity.HasOne(e => e.WeeklyPlanDay)
                  .WithMany(d => d.Exercises)
                  .HasForeignKey(e => e.WeeklyPlanDayId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Exercise)
                  .WithMany()
                  .HasForeignKey(e => e.ExerciseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
