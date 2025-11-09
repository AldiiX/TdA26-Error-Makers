using tda26.Server.Classes.Objects;

namespace tda26.Server.Repositories;

public interface ILecturerRepository {
    Task<Lecturer?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<List<Lecturer>> GetAllAsync(CancellationToken ct = default);
}