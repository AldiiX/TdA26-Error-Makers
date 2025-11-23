using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public class LecturerRepository(
    IAccountRepository accounts
) : ILecturerRepository {

    public async Task<Lecturer?> GetByIdAsync(Guid uuid, CancellationToken ct = default) {
        var account = await accounts.GetByIdAsync(uuid, ct);
        if (account is not Lecturer lecturer) return null;

        if (lecturer is { IsPublic: false }) return null;

        return lecturer;
    }

    public async Task<List<Lecturer>> GetAllAsync(CancellationToken ct = default) {
        var allAccounts = (await accounts.GetAllAsync(ct)).Where(a => a is Lecturer l && l.IsPublic).Cast<Lecturer>().ToList();

        // sortnuti podle createdAt
        allAccounts.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));

        return allAccounts;
    }
}