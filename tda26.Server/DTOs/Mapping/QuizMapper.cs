using tda26.Server.Data.Models;
using tda26.Server.DTOs.v1;

namespace tda26.Server.DTOs.Mapping;

public static class QuizMapper {
    public static ReadQuizResponse ToReadDto(this Quiz quiz, bool extended = false) {
        if (extended) {
            return new ReadQuizResponse {
                Uuid = quiz.Uuid,
                Title = quiz.Title,
                AttemptsCount = quiz.AttemptsCount,
                CreatedAt = quiz.CreatedAt,
                Questions = quiz.Questions
                    .Select(q => (object)q.ToReadDto())
                    .ToList(),
            };
        }
        
        return new ReadQuizResponse {
            Uuid = quiz.Uuid,
            Title = quiz.Title,
            AttemptsCount = quiz.AttemptsCount,
            Questions = quiz.Questions
                .Select(q => (object)q.ToReadDto())
                .ToList(),
        };
    }
}