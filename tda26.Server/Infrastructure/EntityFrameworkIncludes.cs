using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;
using tda26.Server.Data.Models;

namespace tda26.Server.Infrastructure;

public static class EntityFrameworkIncludes {

	public static IQueryable<Course> CoursesMinimalEf(this AppDbContext db) {
		db.CheckScheduling();
		return db.Courses
			.Include(c => c.Tags)
				.ThenInclude(t => t.Category)
			.Include(c => c.Account)
			.Include(c => c.Ratings)
				.ThenInclude(l => l.Account)
			.Include(c => c.Category);
	}

	public static IQueryable<Course> CoursesFullEf(this AppDbContext db) {
		return CoursesMinimalEf(db)
				.Include(c => c.Materials
					.OrderByDescending(m => m.CreatedAt))
				.Include(c => c.Quizzes
					.OrderByDescending(q => q.CreatedAt))
				/*.ThenInclude(q => q.Questions)
					.ThenInclude(qu => qu.Options)*/
				.Include(c => c.Feed)
			;
	}

	public static IQueryable<Account> AccountsEf(this AppDbContext db) {
		return db.Accounts
			.Include(a => a.Ratings)
				.ThenInclude(r => r.Course);
	}

	public static IQueryable<Quiz> QuizzesEf(this AppDbContext db) {
		return db.Quizzes
			.Include(q => q.Questions
				.OrderBy(qs => qs.Order))
			.ThenInclude(qn => qn.Options);
	}

	public static IQueryable<QuizResult> QuizResultsEf(this AppDbContext db) {
		return db.QuizResults
			.Include(qr => qr.Answers)
				.ThenInclude(a => a.SelectedOptions);
	}

	public static IQueryable<FeedPost> FeedPostsEf(this AppDbContext db) {
		return db.FeedPosts
			.Include(fp => fp.Course)
			.Include(fp => fp.Account);
	}
	
	public static void CheckScheduling(this AppDbContext db) {
		var now = DateTime.UtcNow;
		var coursesToStart = db.Courses.Where(c => c.Status == CourseStatus.Scheduled && c.ScheduledStart <= now).ToList();
		foreach (var course in coursesToStart) {
			course.Status = CourseStatus.Live;
		}
		db.SaveChanges();
	}
}