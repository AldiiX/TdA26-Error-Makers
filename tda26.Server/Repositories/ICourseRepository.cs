using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public interface ICourseRepository {
    Task<Course?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    
    Task<List<Course>> GetAllAsync(CancellationToken ct = default);
    
    Task CreateAsync(Course course, CancellationToken ct = default);
    
    Task UpdateAsync(Course course, CancellationToken ct = default);
    
    Task<bool> DeleteAsync(Guid uuid, CancellationToken ct = default);
}