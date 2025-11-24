using tda26.Server.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace tda26.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
    public DbSet<Account> Accounts { get; set; }
    
    public DbSet<Lecturer> Lecturers => Set<Lecturer>();
    
    public DbSet<Course> Courses => Set<Course>();
    
    public DbSet<Material> Materials { get; set; }
    public DbSet<FileMaterial> FileMaterials => Set<FileMaterial>();
    public DbSet<UrlMaterial> UrlMaterials => Set<UrlMaterial>();
    
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    
    public DbSet<FeedPost> FeedItems => Set<FeedPost>();

    // auto update audit properties
    private void SetAuditProperties() {
        var entries = ChangeTracker.Entries().Where(e => e is { Entity: IAuditable, State: EntityState.Added or EntityState.Modified });

        foreach (var entityEntry in entries) {
            ((IAuditable)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;
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