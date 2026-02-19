using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class UpdateUrlMaterialRequest : UpdateMaterialRequest {
    [MaxLength(256)] public string? Url { get; set; }
}
