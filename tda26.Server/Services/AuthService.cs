using System.Text.Json;
using tda26.Server.Classes;
using tda26.Server.Data.Models;
using tda26.Server.Data;
using tda26.Server.DTOs;
using tda26.Server.Repositories;

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
        Console.WriteLine($"Password from session: {sessionAcc.Password}, password from db: {acc?.Password}");
        if (acc == null || acc.Password != sessionAcc.Password) return null;

        http.HttpContext!.Items["loggedaccount"] = acc;
        http.HttpContext!.Session.SetString("loggedaccount", JsonSerializer.Serialize(acc));
        return acc;
    }

    /*public async Task<Account?> RegisterAsync(string username, string email, string plainPassword, string? displayName, CancellationToken ct = default) {
        var hashed = Utilities.EncryptPassword(plainPassword);
        await using var conn = await db.OpenAsync(ct);

        await using var cmd = new NpgsqlCommand(@"
            insert into accounts (uuid, username, display_name, email, password, created_at)
            values (gen_random_uuid(), @u, @d, @e, @p, now());
            select * from accounts where username = @u and email = @e;
        ", conn);

        cmd.Parameters.AddWithValue("u", username);
        cmd.Parameters.AddWithValue("d", (object?)displayName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("e", email);
        cmd.Parameters.AddWithValue("p", hashed);

        try {
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return null;

            return new Account(
                reader.GetGuid("uuid"),
                reader.GetString("username"),
                reader.GetString("display_name"),
                reader.GetString("avatar_url"),
                reader.GetString("email"),
                reader.GetString("password"),
                reader.GetDateTime("created_at"),
                Enum.TryParse<Account.AccountPlan>(reader.GetString("plan"), out var plan) ? plan : Account.AccountPlan.FREE
            );
        } catch (NpgsqlException e) when (e.SqlState == "23505") {
            return null;
        }
    }
    */

    public async Task<Account?> ReAuthFromContextOrNullAsync(CancellationToken ct = default) {
        if (http.HttpContext == null) return null;
        if (!http.HttpContext.Items.ContainsKey("loggedaccount")) return await ReAuthAsync(ct);

        var str = http.HttpContext?.Session.GetString("loggedaccount");
        if (string.IsNullOrEmpty(str)) return null;

        var acc = JsonSerializer.Deserialize<Account>(str, JsonSerializerOptions.Web);
        if (acc != null) return acc;

        return await ReAuthAsync(ct);
    }
}