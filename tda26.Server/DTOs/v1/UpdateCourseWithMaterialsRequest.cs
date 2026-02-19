using tda26.Server.DTOs.v1;

namespace tda26.Server.DTOs.v1;

public class UpdateCourseWithMaterialsRequest {
    public required UpdateCourseRequest Course { get; set; }
    public List<UpdateUrlMaterialRequest> UrlMaterials { get; set; } = new();
    public List<UpdateFileMaterialRequest> FileMaterials { get; set; } = new();
}