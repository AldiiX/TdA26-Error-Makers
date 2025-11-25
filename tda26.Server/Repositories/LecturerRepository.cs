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

    public async Task<List<Lecturer>> GetAllAsync(CancellationToken ct = default) {
        var lecturers = (await db.Lecturers.ToListAsync(ct));

        // sortnuti podle createdat
        lecturers.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));

        //Console.WriteLine(lecturers.ToJsonString());
        return lecturers;
    }
}