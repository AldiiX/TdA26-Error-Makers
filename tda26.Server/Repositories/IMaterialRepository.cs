using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public interface IMaterialRepository {
    Task AddMaterialAsync(Guid courseUuid, Material material, CancellationToken ct = default);
    
    Task<List<Material>> GetMaterialsByCourseIdAsync(Guid courseUuid, CancellationToken ct = default);
}