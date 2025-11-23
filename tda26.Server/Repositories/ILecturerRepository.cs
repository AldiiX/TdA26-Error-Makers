using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public interface ILecturerRepository {
    Task<Lecturer?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<List<Lecturer>> GetAllAsync(CancellationToken ct = default);
}