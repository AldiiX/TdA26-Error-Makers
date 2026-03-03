using tda26.Server.Data.Models;

namespace tda26.Server.DTOs.v1;

public class UpdateCourseStatusRequest {
    public CourseStatus? Status { get; set; }
    public DateTimeOffset? ScheduledStart { get; set; }
}