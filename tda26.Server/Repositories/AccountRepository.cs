using MySqlConnector;
using tda26.Server.Classes.Objects;
using tda26.Server.Infrastructure;
using tda26.Server.Services;

namespace tda26.Server.Repositories;





public class AccountRepository(
    IDatabaseService db
) : IAccountRepository {

    public async Task<Account?> GetByIdAsync(Guid uuid, CancellationToken ct = default) {
        await using var conn = await db.GetOpenConnectionAsync(ct);
        if (conn == null) return null;

        const string query =
            """
            select * from lecturers
            where uuid = @uuid  
            """;

        await using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@uuid", uuid);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        var user = MapAccount(reader);

        return user;
    }

    public async Task<List<Account>> GetAllAsync(CancellationToken ct = default) {
        await using var conn = await db.GetOpenConnectionAsync(ct);
        if (conn == null) return [];

        await using var cmd = new MySqlCommand(
        """
            select * from lecturers
        """, conn);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var list = new List<Account>();
        while (await reader.ReadAsync(ct)) {
            list.Add(MapAccount(reader));
        }

        return list;
    }



    public static Account MapAccount(MySqlDataReader reader) {
        return new Account(
            reader.GetGuid("uuid"),
            reader.GetString("username"),
            reader.GetString("password"),
            reader.GetStringOrNull("title_before"),
            reader.GetString("first_name"),
            reader.GetStringOrNull("middle_name"),
            reader.GetString("last_name"),
            reader.GetStringOrNull("title_after"),
            reader.GetStringOrNull("bio"),
            reader.GetStringOrNull("picture_url"),
            reader.GetStringOrNull("claim"),
            reader.GetValueOrNull<ushort>("price_per_hour") ?? 0,
            reader.GetStringOrNull("mobile_numbers") != null
                ? reader.GetString("mobile_numbers").Split(',').Select(s => s.Trim()).ToList()
                : [],
            reader.GetStringOrNull("emails") != null
                ? reader.GetString("emails").Split(',').Select(s => s.Trim()).ToList()
                : [],
            reader.GetStringOrNull("tags") != null
                ? reader.GetString("tags").Split(',').Select(s => s.Trim()).ToList()
                : [],
            reader.GetStringOrNull("location"),
            reader.GetDateTime("member_since")
        );
    }
}