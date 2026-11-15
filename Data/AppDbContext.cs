using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using TracNghiemWeb.Models;

namespace TracNghiemWeb.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
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
    public DbSet<UploadedDocument> Documents => Set<UploadedDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.DisplayName).IsRequired().HasMaxLength(80);
            entity.HasIndex(u => new { u.IsDeleted, u.CreatedAtUtc });
        });
        modelBuilder.Entity<UploadedDocument>(entity =>
        {
            entity.Property(d => d.Title).IsRequired().HasMaxLength(200);
            entity.Property(d => d.Description).HasMaxLength(1000);
            entity.Property(d => d.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(d => d.StorageKey).IsRequired().HasMaxLength(80);
            entity.Property(d => d.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(d => d.RowVersion).IsRowVersion();
            entity.HasIndex(d => d.StorageKey).IsUnique();
            entity.HasIndex(d => new { d.OwnerId, d.UpdatedAtUtc });
            entity.HasOne(d => d.Owner).WithMany().HasForeignKey(d => d.OwnerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Subject).WithMany().HasForeignKey(d => d.SubjectId).OnDelete(DeleteBehavior.SetNull);
        });
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
            entity.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(a => new { a.UserId, a.SubmittedAt, a.Id });
            entity.HasIndex(a => new { a.SubjectId, a.StartedAt });
            entity.HasIndex(a => new { a.SubmittedAt, a.Id });

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
