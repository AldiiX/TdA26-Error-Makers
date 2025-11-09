using tda26.Server.Classes.Objects;

namespace tda26.Server.Repositories;

public interface IAccountRepository {
    Task<Account?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<List<Account>> GetAllAsync(CancellationToken ct = default);
}