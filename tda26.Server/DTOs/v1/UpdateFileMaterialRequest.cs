using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class UpdateFileMaterialRequest : UpdateMaterialRequest {
    public IFormFile? File { get; set; }
}