namespace tda26.Server.Data.Models;

public interface IAuditable {
    DateTimeOffset UpdatedAt { get; set; }

    DateTimeOffset CreatedAt { get; set; }
}