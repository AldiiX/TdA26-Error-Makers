using System.ComponentModel.DataAnnotations;

namespace tda26.Server.DTOs.v1;

public class AuthRegisterRequest {
    [Required] [MaxLength(64)] public string Username { get; set; } = string.Empty;
    
    [Required] [MaxLength(64)] public string Email { get; set; } = string.Empty;
    [Required] [MaxLength(64)] public string Password { get; set; } = string.Empty;
    
    // Student-specific fields
    [Required] [MaxLength(32)] public string FirstName { get; set; } = string.Empty;
    [MaxLength(32)] public string? MiddleName { get; set; }
    [Required] [MaxLength(32)] public string LastName { get; set; } = string.Empty;

    [Required] public Guid OrganizationUuid { get; set; }
}