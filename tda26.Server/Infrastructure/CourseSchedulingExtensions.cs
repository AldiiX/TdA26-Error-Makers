using Microsoft.EntityFrameworkCore;
using tda26.Server.Controllers;
using tda26.Server.Data;
using tda26.Server.Data.Models;

namespace tda26.Server.Infrastructure;

public static class CourseSchedulingExtensions {
	public async static Task CheckSchedulingAsync(this AppDbContext db, IStreamBroker sb, CancellationToken cancellationToken = default) {
		var now = DateTime.UtcNow;

		var coursesToStart = await db.Courses
			.Where(c => c.Status == CourseStatus.Scheduled && c.ScheduledStart <= now)
			.ToListAsync(cancellationToken);

		foreach (var course in coursesToStart) {
			course.Status = CourseStatus.Live;

			_ = sb.PublishAsync(
				course.Uuid,
				new StreamMessage("status_changed", new { status = course.Status.ToString().ToLower() }),
				cancellationToken
			);
		}

		await db.SaveChangesAsync(cancellationToken);
	}

	public async static Task<bool> CheckSchedulingAsync(this Course course, IStreamBroker sb, CancellationToken cancellationToken = default) {
		var now = DateTime.UtcNow;
		var statusChanged = false;

		if (course.Status == CourseStatus.Scheduled && course.ScheduledStart <= now) {
			course.Status = CourseStatus.Live;
			statusChanged = true;

			_ = sb.PublishAsync(
				course.Uuid,
				new StreamMessage("status_changed", new { status = course.Status.ToString().ToLower() }),
				cancellationToken
			);
		}

		return statusChanged;
	}
}