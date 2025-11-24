using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;
using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public class MaterialRepository(AppDbContext db) : IMaterialRepository {
    public async Task AddMaterialAsync(Guid courseUuid, Material material, CancellationToken ct = default) {
        var course = await db.Courses
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
        if (course == null) {
            throw new Exception("Course not found.");
        }

        material.CourseUuid = course.Uuid;
        db.Materials.Add(material);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<Material>> GetMaterialsByCourseIdAsync(Guid courseUuid, CancellationToken ct = default) {
        var course = await db.Courses
            .FirstOrDefaultAsync(c => c.Uuid == courseUuid, ct);
        if (course == null) {
            throw new Exception("Course not found.");
        }

        return await db.Materials
            .Where(m => m.CourseUuid == course.Uuid)
            .ToListAsync(ct);
    }
}