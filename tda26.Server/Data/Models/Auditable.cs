using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

public class Auditable : IAuditable {
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}