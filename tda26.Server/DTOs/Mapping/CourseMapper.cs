using tda26.Server.Data.Models;
using tda26.Server.DTOs.v1;

namespace tda26.Server.DTOs.Mapping;

public static class CourseMapper
{
    public static ReadCourseResponse ToReadDto(this Course course, bool extended = false)
    {
        // Clear circular references to prevent infinite loop during serialization
        // Following the same pattern as APIv2
        if (course.Account != null) {
            course.Account.Ratings = [];
        }
        
        return new ReadCourseResponse
        {
            Uuid = course.Uuid,
            Name = course.Name,
            Description = course.Description,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            Status = course.Status,
            ImageUrl = course.ImageUrl != null && course.ImageUrl.StartsWith("course-images/")
                ? $"/api/v2/courses/{course.Uuid}/image"
                : null,
            Lecturer = course.Lecturer,
            Account = course.Account,
            ViewCount = course.ViewCount,
            LikeCount = course.LikeCount,
            ImageUrlOrDefault = course.ImageUrlOrDefault,
            Materials = course.Materials.Select(m => m.ToReadDto()).ToList(),
            Quizzes = course.Quizzes.Select(q => q.ToReadDto(extended)).ToList(),
            Feed = course.Feed,
            RatingScore = course.RatingScore,
            Author = course.Author,
            Category = course.Category,
            Tags = course.Tags.ToList()
        };
    }
}