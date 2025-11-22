using tda26.Server.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace tda26.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Lecturer> Lecturers => Set<Lecturer>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<FeedPost> FeedItems => Set<FeedPost>();
    
    // Auto update audit properties
    private void SetAuditProperties()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable && 
                        (e.State == EntityState.Added || 
                         e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            ((IAuditable)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }

    public override int SaveChanges()
    {
        SetAuditProperties();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditProperties();
        return base.SaveChangesAsync(cancellationToken);
    }
}