using tda26.Server.Classes.Objects;

namespace tda26.Server.Repositories;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    
    Task<List<Course>> GetAllAsync(CancellationToken ct = default);
    
    Task<Course?> CreateAsync(string name, string description, CancellationToken ct = default);
    
    Task<bool> UpdateAsync(Course course, CancellationToken ct = default);
    
    Task<bool> DeleteAsync(Guid uuid, CancellationToken ct = default);
}