using System.Data.Common;
using System.Net.Sockets;
using MySqlConnector;

namespace tda26.Server.Services;

public class DatabaseService(
    [FromKeyedServices("main")] MySqlDataSource mainDataSource,
    [FromKeyedServices("fallback")] MySqlDataSource fallbackDataSource,
    ILogger<DatabaseService> logger
) : IDatabaseService {

    public MySqlConnection? GetOpenConnection() {
        try {
            var conn = mainDataSource.CreateConnection();
            conn.Open();
            return conn;
        } catch (Exception ex) when (IsConnectionProblem(ex)) {
            logger.LogWarning(ex, "main db unavailable, switching to fallback (sync)");
            var fb = fallbackDataSource.CreateConnection();
            fb.Open();
            return fb;
        }
    }

    public async ValueTask<MySqlConnection?> GetOpenConnectionAsync(CancellationToken ct = default) {
        try {
            return await mainDataSource.OpenConnectionAsync(ct);
        } catch (Exception ex) when (IsConnectionProblem(ex)) {
            logger.LogWarning(ex, "main db unavailable, switching to fallback (async)");
            return await fallbackDataSource.OpenConnectionAsync(ct);
        }
    }

    private static bool IsConnectionProblem(Exception ex) {
        return ex switch {
            OperationCanceledException => false,
            MySqlException { IsTransient: true } => true,
            MySqlException { Number: 0 } => true,
            _ => ex is TimeoutException or IOException or SocketException or DbException
        };
    }
}