using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class CreateFileMaterialRequest : CreateMaterialRequest {
    public IFormFile? File { get; set; }
}