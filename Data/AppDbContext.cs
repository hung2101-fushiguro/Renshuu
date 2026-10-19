using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.Property(q => q.Content).IsRequired().HasMaxLength(2000);
            entity.Property(q => q.OptionA).IsRequired().HasMaxLength(500);
            entity.Property(q => q.OptionB).IsRequired().HasMaxLength(500);
            entity.Property(q => q.OptionC).IsRequired().HasMaxLength(500);
            entity.Property(q => q.OptionD).IsRequired().HasMaxLength(500);
            entity.Property(q => q.CorrectOption).IsRequired().HasMaxLength(1);
            entity.Property(q => q.Explanation).HasMaxLength(2000);

            entity.HasIndex(q => q.SubjectId);

            entity.HasOne(q => q.Subject)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
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
