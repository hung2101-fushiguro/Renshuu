using Microsoft.EntityFrameworkCore;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<SubjectFolder> SubjectFolders => Set<SubjectFolder>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<ReadingPassage> ReadingPassages => Set<ReadingPassage>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubjectFolder>(entity =>
        {
            entity.Property(f => f.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(f => f.Name).IsUnique();

            entity.HasMany(f => f.Subjects)
                .WithOne(s => s.Folder)
                .HasForeignKey(s => s.FolderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(s => s.FolderId);
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
