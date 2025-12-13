using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

public interface IAuditable {
    DateTime UpdatedAt { get; set; }

    DateTime CreatedAt { get; set; }
}