
using tda26.Server.Classes.Objects;

namespace tda26.Server.Services;

public interface IAuthService {
    Task<Account?> LoginAsync(string identifier, string plainPassword, CancellationToken ct = default);
    Task<Account?> ReAuthAsync(CancellationToken ct = default);
    Task<Account?> ReAuthFromContextOrNullAsync(CancellationToken ct = default);
}