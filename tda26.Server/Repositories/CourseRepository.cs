using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;
using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public class CourseRepository(AppDbContext db) : ICourseRepository {
    public async Task<Course?> GetByIdAsync(Guid uuid, CancellationToken ct = default) {
        return await db.Courses
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
    }
    
    public async Task<Course?> GetByIdAsyncFull(Guid uuid, CancellationToken ct = default) {
        return await db.Courses
            .Include(c => c.Tags)
            .Include(c => c.Materials)
            .Include(c => c.Quizzes)
            .Include(c => c.Feed)
            .FirstOrDefaultAsync(c => c.Uuid == uuid, ct);
    }

    public async Task<List<Course>> GetAllAsync(CancellationToken ct = default) {
        return await db.Courses
            .Include(c => c.Tags)
            .ToListAsync(ct);
    }
    
    public async Task<List<Course>> GetAllAsyncFull(CancellationToken ct = default) {
        return await db.Courses
            .Include(c => c.Tags)
            .Include(c => c.Materials)
            .Include(c => c.Quizzes)
            .Include(c => c.Feed)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(Course course, CancellationToken ct = default) {
        db.Courses.Add(course);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Course course, CancellationToken ct = default) {
        db.Courses.Update(course);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid uuid, CancellationToken ct = default) {
        var course = await GetByIdAsync(uuid, ct);
        if (course == null) return false;
        
        db.Courses.Remove(course);
        await db.SaveChangesAsync(ct);
            
        return true;
    }
}