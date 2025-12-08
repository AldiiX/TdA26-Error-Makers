using tda26.Server.Data.Models;
using tda26.Server.DTOs.v1;

namespace tda26.Server.DTOs.Mapping;

public static class CourseMapper
{
    public static ReadCourseResponse ToReadDto(this Course course) =>
        new()
        {
            Uuid = course.Uuid,
            Name = course.Name,
            Description = course.Description,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            ImageUrl = course.ImageUrl,
            Lecturer = course.Lecturer,
            Materials = course.Materials.Select(m => m.ToReadDto()).ToList(),
            Quizzes = course.Quizzes.Select(q => q.ToReadDto()).ToList(),
            Feed = course.Feed.Select(f => new ReadFeedResponse { /* map */ }).ToList()
        };
}