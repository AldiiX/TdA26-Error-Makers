using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;
using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public class LecturerRepository(
    IAccountRepository accounts,
    AppDbContext db
) : ILecturerRepository {

    public async Task<Lecturer?> GetByIdAsync(Guid uuid, CancellationToken ct = default) {
        var lecturer = await db.Lecturers
            .FirstOrDefaultAsync(l => l.Uuid == uuid, ct);

        return lecturer;
    }

    public async Task<List<Lecturer>> GetAllAsync(uint limit = 0, CancellationToken ct = default) {
        var isLimited = limit > 0;

        var lecturers = await db.Lecturers
            .OrderBy(l => l.CreatedAt)
            .Take(isLimited ? (int) limit : int.MaxValue)
            .ToListAsync(ct);
        
        return lecturers;
    }
}