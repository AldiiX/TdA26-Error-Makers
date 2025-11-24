using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class CreateFileMaterialRequest : CreateMaterialRequest {
    [Required] [MaxLength(256)] public required string FileUrl { get; set; }
}