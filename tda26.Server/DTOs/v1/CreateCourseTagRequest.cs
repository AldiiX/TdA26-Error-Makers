namespace tda26.Server.DTOs.v1;

public class CreateCourseTagRequest {
    public required string DisplayName { get; set; }
    public required Guid CategoryUuid { get; set; }
}