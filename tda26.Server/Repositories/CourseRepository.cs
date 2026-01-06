using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;
using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public class CourseRepository(AppDbContext db) : ICourseRepository {
    public async Task<Course?> GetByUuidAsync(Guid uuid, CancellationToken ct = default) {
        var course = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Account)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);

        return course;
    }
    
    public async Task<Course?> GetByUuidAsyncFull(Guid uuid, CancellationToken ct = default) {
        var course = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Account)
            .Include(c => c.Materials)
            .Include(c => c.Quizzes)
            .Include(c => c.Feed)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);

        return course;
    }

    public async Task<List<Course>> GetAllAsync(uint limit = 0, CancellationToken ct = default) {
        var isLimited = limit > 0;

        var courses = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Account)
            .Include(c => c.Category)
            .OrderByDescending(c => c.CreatedAt)
            .Take(isLimited ? (int) limit : int.MaxValue)
            .ToListAsync(ct);

        return courses;
    }

    public async Task<List<Course>> GetAllAsyncFull(uint limit = 0, CancellationToken ct = default) {
        var isLimited = limit > 0;

        var courses = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Account)
            .Include(c => c.Materials)
            .Include(c => c.Quizzes
                .OrderByDescending(q => q.CreatedAt))
            .Include(c => c.Feed)
            .Include(c => c.Category)
            .OrderByDescending(c => c.CreatedAt)
            .Take(isLimited ? (int) limit : int.MaxValue)
            .ToListAsync(ct);
        
        return courses;
    }

    public async Task<List<Course>> GetByLecturerUuidAsync(Guid lecturerUuid, int max = -1, CancellationToken ct = default) {
        var courses = await db.Courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Category)
            .Where(c => c.LecturerUuid == lecturerUuid)
            .OrderByDescending(c => c.CreatedAt)
            .Take(max > -1 ? max : int.MaxValue)
            .ToListAsync(ct);

        return courses;
    }

    public async Task<List<Course>> GetByLecturerUuidAsyncFull(Guid lecturerUuid, int max = -1, CancellationToken ct = default) {
        var courses = await db.Courses
            .Where(c => c.LecturerUuid == lecturerUuid)
            .Include(c => c.Ratings)
            .ThenInclude(l => l.Account)
            .Include(c => c.Account)
            .Include(c => c.Materials)
            .Include(c => c.Quizzes)
            .Include(c => c.Feed)
            .Include(c => c.Category)
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .OrderByDescending(c => c.CreatedAt)
            .Take(max > -1 ? max : int.MaxValue)
            .ToListAsync(ct);

        return courses;
    }

    public async Task CreateAsync(Course course, CancellationToken ct = default) {
        db.Courses.Add(course);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Course course, CancellationToken ct = default) {
        // Check if entity is already being tracked by EF
        var entry = db.Entry(course);
        
        // If the entity is not being tracked (Detached), we need to attach/update it
        if (entry.State == EntityState.Detached) {
            db.Courses.Update(course);
        }
        // If entity is already tracked, EF will automatically detect property changes during SaveChangesAsync,
        // allowing SetAuditProperties to properly identify which specific properties changed.
        
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid uuid, CancellationToken ct = default) {
        var course = await GetByUuidAsync(uuid, ct);
        if (course == null) return false;
        
        db.Courses.Remove(course);
        await db.SaveChangesAsync(ct);
            
        return true;
    }
}