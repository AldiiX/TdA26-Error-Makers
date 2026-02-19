namespace tda26.Server.DTOs.v1;

public class CreateCourseWithMaterialsRequest {
    public CreateCourseRequest Course { get; set; } = null!;
    public List<CreateUrlMaterialRequest> UrlMaterials { get; set; } = new();
    public List<CreateFileMaterialRequest> FileMaterials { get; set; } = new();
}