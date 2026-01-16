using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

public class Auditable : IAuditable {
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset UpdatedAt { get; set; } = DateTime.UtcNow;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
}