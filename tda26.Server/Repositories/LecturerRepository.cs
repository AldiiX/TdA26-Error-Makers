using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public class LecturerRepository(
    IAccountRepository accounts
) : ILecturerRepository {

    public async Task<Lecturer?> GetByIdAsync(Guid uuid, CancellationToken ct = default) {
        var account = await accounts.GetByIdAsync(uuid, ct);
        return account as Lecturer;
    }

    public async Task<List<Lecturer>> GetAllAsync(CancellationToken ct = default) {
        var allAccounts = await accounts.GetAllAsync(ct);
        return allAccounts.OfType<Lecturer>().ToList();
    }
}