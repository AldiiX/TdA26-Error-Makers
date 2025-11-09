using tda26.Server.Classes.Objects;

namespace tda26.Server.Repositories;

public class LecturerRepository(
    IAccountRepository accounts
) : ILecturerRepository {

    public async Task<Lecturer?> GetByIdAsync(Guid uuid, CancellationToken ct = default) {
        var acc = await accounts.GetByIdAsync(uuid, ct);

        return acc;
    }

    public async Task<List<Lecturer>> GetAllAsync(CancellationToken ct = default) {
        var all = await accounts.GetAllAsync(ct);
        var lecturers = all.OfType<Lecturer>().ToList();
        return lecturers;
    }
}