namespace tda26.Server.Data.Models;

public interface IAuditable {
    DateTime UpdatedAt { get; set; }

    DateTime CreatedAt { get; set; }
}