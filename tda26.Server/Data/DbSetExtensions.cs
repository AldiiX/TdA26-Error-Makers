using Microsoft.EntityFrameworkCore;
using tda26.Server.Data.Models;

namespace tda26.Server.Data;

/// <summary>
/// Extension methods for DbSet to provide convenient IncludeAll() functionality
/// </summary>
public static class DbSetExtensions {
    /// <summary>
    /// Includes all related entities for Course (including Materials, Quizzes, and Feed)
    /// </summary>
    public static IQueryable<Course> IncludeAll(this DbSet<Course> courses) {
        return ((IQueryable<Course>)courses).IncludeAll();
    }

    /// <summary>
    /// Includes all related entities for Course (including Materials, Quizzes, and Feed)
    /// </summary>
    public static IQueryable<Course> IncludeAll(this IQueryable<Course> courses) {
        return courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Ratings)
            .Include(c => c.Account)
            .Include(c => c.Materials)
            .Include(c => c.Quizzes)
            .Include(c => c.Feed)
            .Include(c => c.Category);
    }

    /// <summary>
    /// Includes basic related entities for Course (excludes Materials, Quizzes, and Feed for performance)
    /// </summary>
    public static IQueryable<Course> IncludeBasic(this DbSet<Course> courses) {
        return ((IQueryable<Course>)courses).IncludeBasic();
    }

    /// <summary>
    /// Includes basic related entities for Course (excludes Materials, Quizzes, and Feed for performance)
    /// </summary>
    public static IQueryable<Course> IncludeBasic(this IQueryable<Course> courses) {
        return courses
            .Include(c => c.Tags)
            .ThenInclude(t => t.Category)
            .Include(c => c.Ratings)
            .Include(c => c.Account)
            .Include(c => c.Category);
    }

    /// <summary>
    /// Includes all related entities for Account
    /// </summary>
    public static IQueryable<Account> IncludeAll(this DbSet<Account> accounts) {
        return ((IQueryable<Account>)accounts).IncludeAll();
    }

    /// <summary>
    /// Includes all related entities for Account
    /// </summary>
    public static IQueryable<Account> IncludeAll(this IQueryable<Account> accounts) {
        return accounts
            .Include(a => a.Ratings);
    }
}
