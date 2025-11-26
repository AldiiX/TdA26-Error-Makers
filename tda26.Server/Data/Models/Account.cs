using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace tda26.Server.Data.Models;

[Index(nameof(Username), IsUnique = true)]
public class Account : Auditable {
    [Key]
    public Guid Uuid { get; set; } = Guid.NewGuid();

    [MaxLength(32)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(512), JsonIgnore]
    public string Password { get; set; } = string.Empty;
}