using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

[Obsolete]
public interface ILecturerRepository {
    Task<Lecturer?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<List<Lecturer>> GetAllAsync(uint limit = 0, CancellationToken ct = default);
}