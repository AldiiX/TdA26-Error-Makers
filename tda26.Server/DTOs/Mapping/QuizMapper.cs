using tda26.Server.Data.Models;
using tda26.Server.DTOs.v1;

namespace tda26.Server.DTOs.Mapping;

public static class QuizMapper {
    public static ReadQuizResponse ToReadDto(this Quiz quiz)
    {
        return new ReadQuizResponse
        {
            Uuid = quiz.Uuid,
            Title = quiz.Title,
            AttemptsCount = quiz.AttemptsCount,
            Questions = quiz.Questions
                .Select(q => (object)q.ToReadDto())
                .ToList()
        };
    }
}