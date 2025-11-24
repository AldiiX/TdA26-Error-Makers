using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class CreateUrlMaterialRequest : CreateMaterialRequest {
    [Required] [MaxLength(256)] public required string Url { get; set; }
}