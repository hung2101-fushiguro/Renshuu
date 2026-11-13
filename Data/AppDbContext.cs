using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public string? CurrentUserId => _http?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    private readonly IHttpContextAccessor? _http;
    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? http = null) : base(options)
    {
        _http = http;
    }

    public DbSet<LearnerProfile> LearnerProfiles => Set<LearnerProfile>();
    public DbSet<SavedQuestion> SavedQuestions => Set<SavedQuestion>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<ReadingPassage> ReadingPassages => Set<ReadingPassage>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<LearnerProfile>().HasQueryFilter(p => p.UserId == CurrentUserId);
        modelBuilder.Entity<LearnerProfile>().HasOne<IdentityUser>().WithOne().HasForeignKey<LearnerProfile>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SavedQuestion>().HasQueryFilter(p => p.UserId == CurrentUserId);
        modelBuilder.Entity<SavedQuestion>().HasIndex(p => new { p.UserId, p.QuestionId }).IsUnique();
        modelBuilder.Entity<SavedQuestion>().HasOne<IdentityUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SavedQuestion>().HasOne(p => p.Question).WithMany().HasForeignKey(p => p.QuestionId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Attempt>().HasQueryFilter(a => CurrentUserId != null && a.UserId == CurrentUserId);
        modelBuilder.Entity<AttemptAnswer>().HasQueryFilter(a => CurrentUserId != null && a.Attempt!.UserId == CurrentUserId);
        modelBuilder.Entity<Attempt>().HasOne<IdentityUser>().WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<ReadingPassage>(entity =>
        {
            entity.Property(p => p.Title).HasMaxLength(300);
            entity.Property(p => p.Content).IsRequired().HasMaxLength(20000);

            entity.HasIndex(p => p.SubjectId);

            entity.HasOne(p => p.Subject)
                .WithMany(s => s.ReadingPassages)
                .HasForeignKey(p => p.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.Property(q => q.Content).IsRequired().HasMaxLength(2000);
            entity.Property(q => q.OptionA).IsRequired().HasMaxLength(500);
            entity.Property(q => q.OptionB).IsRequired().HasMaxLength(500);
            entity.Property(q => q.OptionC).IsRequired().HasMaxLength(500);
            entity.Property(q => q.OptionD).IsRequired().HasMaxLength(500);
            entity.Property(q => q.CorrectOption).IsRequired().HasMaxLength(1);
            entity.Property(q => q.OptionStyle).IsRequired().HasMaxLength(4).HasDefaultValue("ABCD");
            entity.Property(q => q.Explanation).HasMaxLength(2000);

            entity.HasIndex(q => q.SubjectId);
            entity.HasIndex(q => q.PassageId);

            entity.HasOne(q => q.Subject)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(q => q.Passage)
                .WithMany(p => p.Questions)
                .HasForeignKey(q => q.PassageId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Attempt>(entity =>
        {
            entity.HasIndex(a => new { a.SubjectId, a.StartedAt });

            entity.HasOne(a => a.Subject)
                .WithMany(s => s.Attempts)
                .HasForeignKey(a => a.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AttemptAnswer>(entity =>
        {
            entity.Property(a => a.SelectedOption).HasMaxLength(1);

            entity.HasIndex(a => a.AttemptId);
            entity.HasIndex(a => new { a.QuestionId, a.Id });

            entity.HasOne(a => a.Attempt)
                .WithMany(a => a.Answers)
                .HasForeignKey(a => a.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Question)
                .WithMany(q => q.AttemptAnswers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
