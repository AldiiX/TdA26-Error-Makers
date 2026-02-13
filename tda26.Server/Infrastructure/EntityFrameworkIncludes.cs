using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;
using tda26.Server.Data.Models;

namespace tda26.Server.Infrastructure;

public static class EntityFrameworkIncludes {

	/// <summary>
	/// Includes all basic Course relationships: Tags (+ Category), Ratings (+ Account), Account, Category
	/// </summary>
	public static IQueryable<Course> IncludeBasic(this IQueryable<Course> courses) {
		return courses
			.Include(c => c.Tags)
				.ThenInclude(t => t.Category)
			.Include(c => c.Account)
			.Include(c => c.Ratings)
				.ThenInclude(l => l.Account)
			.Include(c => c.Category);
	}

	/// <summary>
	/// Includes all Course relationships: Tags, Ratings, Account, Category, Materials, Quizzes, Feed
	/// </summary>
	public static IQueryable<Course> IncludeAll(this IQueryable<Course> courses) {
		return courses.IncludeBasic()
			.Include(c => c.Materials
				.OrderByDescending(m => m.CreatedAt))
			.Include(c => c.Quizzes
				.OrderByDescending(q => q.CreatedAt))
			.Include(c => c.Feed);
	}

	/// <summary>
	/// Includes Account relationships: Ratings (+ Course)
	/// </summary>
	public static IQueryable<Account> IncludeAll(this IQueryable<Account> accounts) {
		return accounts
			.Include(a => a.Ratings)
				.ThenInclude(r => r.Course);
	}

	/// <summary>
	/// Includes Quiz relationships: Questions (+ Options)
	/// </summary>
	public static IQueryable<Quiz> IncludeAll(this IQueryable<Quiz> quizzes) {
		return quizzes
			.Include(q => q.Questions)
				.ThenInclude(qn => qn.Options);
	}

	/// <summary>
	/// Includes QuizResult relationships: Answers (+ SelectedOptions)
	/// </summary>
	public static IQueryable<QuizResult> IncludeAll(this IQueryable<QuizResult> quizResults) {
		return quizResults
			.Include(qr => qr.Answers)
				.ThenInclude(a => a.SelectedOptions);
	}

	// Backwards compatibility with old method names
	[Obsolete("Use db.Courses.IncludeBasic() instead", error: false)]
	public static IQueryable<Course> CoursesMinimalEf(this AppDbContext db) {
		return db.Courses.IncludeBasic();
	}

	[Obsolete("Use db.Courses.IncludeAll() instead", error: false)]
	public static IQueryable<Course> CoursesFullEf(this AppDbContext db) {
		return db.Courses.IncludeAll();
	}

	[Obsolete("Use db.Accounts.IncludeAll() instead", error: false)]
	public static IQueryable<Account> AccountsEf(this AppDbContext db) {
		return db.Accounts.IncludeAll();
	}
}