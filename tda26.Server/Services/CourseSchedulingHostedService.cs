using tda26.Server.Controllers;
using tda26.Server.Data;
using tda26.Server.Infrastructure;

namespace tda26.Server.Services;

public sealed class CourseSchedulingHostedService(
	IServiceScopeFactory scopeFactory,
	ILogger<CourseSchedulingHostedService> logger
) : BackgroundService {
	protected async override Task ExecuteAsync(CancellationToken stoppingToken) {
		while (!stoppingToken.IsCancellationRequested) {
			var now = DateTime.UtcNow;
			var nextTick = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Utc).AddMinutes(1);
			var delay = nextTick - now;

			// pockej na dalsi celou minutu (xx:yy:00)
			if (delay > TimeSpan.Zero) {
				await Task.Delay(delay, stoppingToken);
			}

			try {
				using var scope = scopeFactory.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
				var sb = scope.ServiceProvider.GetRequiredService<IStreamBroker>();

				await db.CheckSchedulingAsync(sb, stoppingToken);
			} catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
				// korektni ukonceni
				break;
			} catch (Exception ex) {
				logger.LogError(ex, "course scheduling check failed");
			}
		}
	}
}