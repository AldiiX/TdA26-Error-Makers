using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

public class Auditable : IAuditable {
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}