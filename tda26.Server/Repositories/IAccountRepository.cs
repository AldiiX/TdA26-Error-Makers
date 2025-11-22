using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public interface IAccountRepository {
    Task<Account?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<List<Account>> GetAllAsync(CancellationToken ct = default);
    Task CreateAsync(Account account, CancellationToken ct = default);
    Task UpdateAsync(Account account, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid uuid, CancellationToken ct = default);
}