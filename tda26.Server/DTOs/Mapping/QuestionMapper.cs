using tda26.Server.Data.Models;
using tda26.Server.DTOs.v1;

namespace tda26.Server.DTOs.Mapping;

public static class QuestionMapper {
    public static ReadQuestionResponse ToReadDto(this Question question) =>
        question switch
        {
            SingleChoiceQuestion single => new ReadSingleChoiceQuestionResponse
            {
                Uuid = single.Uuid,
                Type = "singleChoice",
                Question = single.Text,
                Options = single.Options
                    .OrderBy(o => o.Uuid)
                    .Select(o => o.Text)
                    .ToList(),

                CorrectIndex = single.Options
                    .OrderBy(o => o.Uuid)
                    .Select((opt, idx) => new { opt, idx })
                    .First(x => x.opt.IsCorrect).idx
            },

            MultipleChoiceQuestion multi => new ReadMultipleChoiceQuestionResponse
            {
                Uuid = multi.Uuid,
                Type = "multipleChoice",
                Question = multi.Text,
                Options = multi.Options
                    .OrderBy(o => o.Uuid)
                    .Select(o => o.Text)
                    .ToList(),

                CorrectIndices = multi.Options
                    .OrderBy(o => o.Uuid)
                    .Select((opt, idx) => new { opt, idx })
                    .Where(x => x.opt.IsCorrect)
                    .Select(x => x.idx)
                    .ToList()
            },

            _ => throw new InvalidOperationException($"Unknown question type: {question.GetType().Name}")
        };
}