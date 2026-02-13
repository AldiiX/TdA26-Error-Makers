using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;
using tda26.Server.DTOs;
using tda26.Server.Infrastructure;
using Account = tda26.Server.Data.Models.Account;

namespace tda26.Server.Services;





public class AuthService(
    AppDbContext db,
    IHttpContextAccessor http
) : IAuthService {


    public async Task<Account?> LoginAsync(string identifier, string plainPassword, CancellationToken ct = default) {
        var acc = await db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Username.ToLower() == identifier.ToLower(), ct);
        if (acc == null) return null;

        var json = new AccountSessionDto {
            Uuid =  acc.Uuid,
            Username = acc.Username,
            Password = acc.Password,
            CreatedAt = acc.CreatedAt,
            UpdatedAt = acc.UpdatedAt,
        };

        //Console.WriteLine("Login attempt for json: " + json);

        if (!Utilities.VerifyPassword(plainPassword, acc.Password)) return null;
        http.HttpContext!.Session.SetString("loggedaccount", JsonSerializer.Serialize(json));
        http.HttpContext.Items["loggedaccount"] = json;
        
        // Load full account with relationships for the response
        return await db.Accounts
            .IncludeAll()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Uuid == acc.Uuid, ct);
    }
    
    // FIX:

    public async Task<Account?> ReAuthAsync(CancellationToken ct = default) {
        var json = http.HttpContext?.Session.GetString("loggedaccount");
        if (string.IsNullOrEmpty(json)) return null;

        var sessionAcc = JsonSerializer.Deserialize<AccountSessionDto>(json);
        if (sessionAcc == null) return null;

        // First check password without loading relationships
        var accLight = await db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Uuid == sessionAcc.Uuid, ct);
        if (accLight == null || accLight.Password != sessionAcc.Password) return null;

        // Keep session data consistent by storing AccountSessionDto instead of full Account
        var sessionDto = new AccountSessionDto {
            Uuid = accLight.Uuid,
            Username = accLight.Username,
            Password = accLight.Password,
            CreatedAt = accLight.CreatedAt,
            UpdatedAt = accLight.UpdatedAt,
        };
        http.HttpContext!.Items["loggedaccount"] = sessionDto;
        http.HttpContext!.Session.SetString("loggedaccount", JsonSerializer.Serialize(sessionDto));
        
        // Load full account with relationships for the response
        return await db.Accounts
            .IncludeAll()
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Uuid == sessionAcc.Uuid, ct);
    }

    public async Task<Account?> RegisterAsync(string username, string email, string plainPassword,  CancellationToken ct = default)
    {
        var existing = await db.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Username.ToLower() == username.ToLower(), ct);
        if (existing != null) return null;

        var hashedPassword = Utilities.HashPassword(plainPassword);

        var acc = new Account {
            Uuid = Guid.NewGuid(),
            Username = username,
            PrimaryEmail = email,
            Password = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Accounts.Add(acc);
        await db.SaveChangesAsync(ct);

        var json = new AccountSessionDto {
            Uuid = acc.Uuid,
            Username = acc.Username,
            Password = acc.Password,
            CreatedAt = acc.CreatedAt,
            UpdatedAt = acc.UpdatedAt
        };

        http.HttpContext!.Session.SetString("loggedaccount", JsonSerializer.Serialize(json));
        http.HttpContext.Items["loggedaccount"] = json;

        // For newly registered accounts, there are no relationships yet, so return the created entity
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