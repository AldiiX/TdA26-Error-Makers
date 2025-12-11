using System.Text.Json;
using tda26.Server.Data.Models;
using tda26.Server.Data;
using tda26.Server.DTOs;
using tda26.Server.Infrastructure;
using tda26.Server.Repositories;
using Account = tda26.Server.Data.Models.Account;

namespace tda26.Server.Services;





public class AuthService(
    AppDbContext db,
    IHttpContextAccessor http,
    IAccountRepository accounts
) : IAuthService {


    public async Task<Account?> LoginAsync(string identifier, string plainPassword, CancellationToken ct = default) {
        var acc = (await accounts.GetAllAsync(ct)).FirstOrDefault(a => a.Username.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        if (acc == null) return null;

        var json = new AccountSessionDto {
            Uuid =  acc.Uuid,
            Username = acc.Username,
            Password = acc.Password,
            CreatedAt = acc.CreatedAt,
            UpdatedAt = acc.UpdatedAt,
        };

        Console.WriteLine("Login attempt for json: " + json);

        if (!Utilities.VerifyPassword(plainPassword, acc.Password)) return null;
        http.HttpContext!.Session.SetString("loggedaccount", JsonSerializer.Serialize(json));
        http.HttpContext.Items["loggedaccount"] = json;
        return acc;
    }
    
    // FIX:

    public async Task<Account?> ReAuthAsync(CancellationToken ct = default) {
        var json = http.HttpContext?.Session.GetString("loggedaccount");
        if (string.IsNullOrEmpty(json)) return null;

        var sessionAcc = JsonSerializer.Deserialize<AccountSessionDto>(json);
        if (sessionAcc == null) return null;

        var acc = await accounts.GetByIdAsync(sessionAcc.Uuid, ct);
        if (acc == null || acc.Password != sessionAcc.Password) return null;

        http.HttpContext!.Items["loggedaccount"] = acc;
        // Keep session data consistent by storing AccountSessionDto instead of full Account
        var sessionDto = new AccountSessionDto {
            Uuid = acc.Uuid,
            Username = acc.Username,
            Password = acc.Password,
            CreatedAt = acc.CreatedAt,
            UpdatedAt = acc.UpdatedAt,
        };
        http.HttpContext!.Session.SetString("loggedaccount", JsonSerializer.Serialize(sessionDto));
        return acc;
    }

    public async Task<Account?> RegisterAsync(string username, string email, string plainPassword,  CancellationToken ct = default)
    {
        var existing = await accounts.GetByUsernameAsync(username, ct);
        if (existing != null) return null;

        var hashedPassword = Utilities.HashPassword(plainPassword);

        var acc = new Account {
            Uuid = Guid.NewGuid(),
            Username = username,
            PrimaryEmail = email,
            Password = hashedPassword,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await accounts.CreateAsync(acc, ct);

        var json = new AccountSessionDto {
            Uuid = acc.Uuid,
            Username = acc.Username,
            Password = acc.Password,
            CreatedAt = acc.CreatedAt,
            UpdatedAt = acc.UpdatedAt
        };

        http.HttpContext!.Session.SetString("loggedaccount", JsonSerializer.Serialize(json));
        http.HttpContext.Items["loggedaccount"] = json;

        return acc;
    }


    public async Task<Account?> ReAuthFromContextOrNullAsync(CancellationToken ct = default) {
        if (http.HttpContext == null) return null;
        if (!http.HttpContext.Items.ContainsKey("loggedaccount")) return await ReAuthAsync(ct);

        // Session always contains AccountSessionDto, so we need to re-auth to get full Account
        return await ReAuthAsync(ct);
    }
    
    public async Task<bool> LogoutAsync(CancellationToken ct = default) {
        if (http.HttpContext == null) return false;
        http.HttpContext.Items.Remove("loggedaccount");
        http.HttpContext.Session.Remove("loggedaccount");
        return true;
    }
}