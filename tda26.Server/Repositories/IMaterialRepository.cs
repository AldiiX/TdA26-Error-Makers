using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

[Obsolete]
public interface IMaterialRepository {
    Task<Material?> GetMaterialByUuidAsync(Guid materialUuid, CancellationToken ct = default);
    
    Task AddMaterialAsync(Guid courseUuid, Material material, CancellationToken ct = default);
    
    Task<List<Material>> GetMaterialsByCourseIdAsync(Guid courseUuid, CancellationToken ct = default);
    
    Task UpdateMaterialAsync(Material material, CancellationToken ct = default);
    
    Task DeleteMaterialAsync(Material material, CancellationToken ct = default);
}