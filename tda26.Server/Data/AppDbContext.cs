using tda26.Server.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace tda26.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
    public DbSet<Account> Accounts { get; set; }
    
    public DbSet<Lecturer> Lecturers => Set<Lecturer>();

    public DbSet<Admin> Admins => Set<Admin>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Category> Categories { get; set; }

    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Dislike> Dislikes => Set<Dislike>();

    public DbSet<Material> Materials { get; set; }
    public DbSet<FileMaterial> FileMaterials => Set<FileMaterial>();
    public DbSet<UrlMaterial> UrlMaterials => Set<UrlMaterial>();
    
    public DbSet<Quiz> Quizzes => Set<Quiz>();

    public DbSet<Question> Questions => Set<Question>();
    public DbSet<SingleChoiceQuestion> SingleChoiceQuestions => Set<SingleChoiceQuestion>();
    public DbSet<MultipleChoiceQuestion> MultipleChoiceQuestions => Set<MultipleChoiceQuestion>();

    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();

    public DbSet<QuizResult> QuizResults => Set<QuizResult>();
    public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();
    public DbSet<QuizAnswerOption> QuizAnswerOptions => Set<QuizAnswerOption>();
    
    public DbSet<FeedPost> FeedPosts => Set<FeedPost>();

    // auto update audit properties
    private void SetAuditProperties() {
        var entries = ChangeTracker.Entries().Where(e => e is { Entity: IAuditable, State: EntityState.Added or EntityState.Modified });

        foreach (var entityEntry in entries) {
            // For newly added entities, always set UpdatedAt
            if (entityEntry.State == EntityState.Added) {
                ((IAuditable)entityEntry.Entity).UpdatedAt = DateTime.Now;
                continue;
            }

            // For modified entities, check which properties were actually modified
            // Skip UpdatedAt update if only ViewCount or navigation properties changed
            var modifiedProperties = entityEntry.Properties
                .Where(p => p.IsModified)
                .Select(p => p.Metadata.Name)
                .ToHashSet();

            // If only ViewCount changed, don't update UpdatedAt (metric changes shouldn't trigger timestamp updates)
            if (entityEntry.Entity is Course && modifiedProperties.Count == 1 && modifiedProperties.Contains(nameof(Course.ViewCount))) {
                continue;
            }

            // If no scalar properties were actually modified (e.g., only navigation properties like Ratings changed
            // when Likes/Dislikes are added/removed), don't update UpdatedAt
            if (modifiedProperties.Count == 0) {
                continue;
            }

            ((IAuditable)entityEntry.Entity).UpdatedAt = DateTime.Now;
        }
    }

    public override int SaveChanges() {
        SetAuditProperties();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        SetAuditProperties();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        /*modelBuilder.Entity<Lecturer>()
            .Property(l => l.IsPublic)
            .IsRequired()
            .HasDefaultValue(true);*/
    }
}