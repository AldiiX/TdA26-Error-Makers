using Microsoft.EntityFrameworkCore;
using tda26.Server.Data;
using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public class QuizRepository(AppDbContext db) : IQuizRepository {
    public async Task<List<Quiz>> GetAllQuizzesFromCourseAsync(Guid courseUuid) {
        var quizzes = await db.Quizzes
            .Where(q => q.CourseUuid == courseUuid)
            .ToListAsync();

        return quizzes ?? throw new KeyNotFoundException("Quiz not found for the specified course.");

    }
    
    public async Task<List<Quiz>> GetAllQuizzesFromCourseAsyncFull(Guid courseUuid) {
        var quizzes = await db.Quizzes
            .Where(q => q.CourseUuid == courseUuid)
            .Include(q => q.Questions)
                .ThenInclude(qn => qn.Options)
            .ToListAsync();

        return quizzes ?? throw new KeyNotFoundException("Quiz not found for the specified course.");

    }
    
    public async Task CreateAsync(Quiz quiz) {
        await db.Quizzes.AddAsync(quiz);
        await db.SaveChangesAsync();
    }
}