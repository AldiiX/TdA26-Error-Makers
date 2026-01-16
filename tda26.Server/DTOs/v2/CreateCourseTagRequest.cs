namespace tda26.Server.DTOs.v2;

public class CreateCourseTagRequest {
    public required string DisplayName { get; set; }
    public required Guid CategoryUuid { get; set; }
}