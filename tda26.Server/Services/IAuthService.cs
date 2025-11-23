
using tda26.Server.Data.Models;

namespace tda26.Server.Services;

public interface IAuthService {
    Task<Account?> LoginAsync(string identifier, string plainPassword, CancellationToken ct = default);
    Task<Account?> ReAuthAsync(CancellationToken ct = default);
    Task<Account?> ReAuthFromContextOrNullAsync(CancellationToken ct = default);
    Task<bool> LogoutAsync(CancellationToken ct = default);
}