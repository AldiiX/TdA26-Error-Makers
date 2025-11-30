using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public interface IQuizRepository {
    Task<List<Quiz>> GetAllQuizzesFromCourseAsync(Guid courseUuid);
    Task<List<Quiz>> GetAllQuizzesFromCourseAsyncFull(Guid courseUuid);
    Task CreateAsync(Quiz quiz);
}