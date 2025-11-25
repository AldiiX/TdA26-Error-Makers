using tda26.Server.Data.Models;

namespace tda26.Server.Repositories;

public interface ICourseRepository {
    /// <summary>
    /// Gets a course by its UUID. Does not include related entities.
    /// </summary>
    Task<Course?> GetByUuidAsync(Guid uuid, CancellationToken ct = default);
    /// <summary>
    /// Gets a course by its UUID, including related entities (Materials, Quizzes, Feed).
    /// </summary>
    Task<Course?> GetByUuidAsyncFull(Guid uuid, CancellationToken ct = default);
    
    /// <summary>
    /// Gets all courses. Does not include related entities.
    /// </summary>
    Task<List<Course>> GetAllAsync(CancellationToken ct = default);
    /// <summary>
    /// Gets all courses, including related entities (Materials, Quizzes, Feed).
    /// </summary>
    Task<List<Course>> GetAllAsyncFull(CancellationToken ct = default);
    
    /// <summary>
    /// Gets all courses taught by a specific lecturer identified by their UUID.
    /// </summary>
    Task<List<Course>> GetByLecturerUuidAsync(Guid lecturerUuid, CancellationToken ct = default);
    
    /// <summary>
    /// Gets all courses taught by a specific lecturer identified by their UUID, including related entities (Materials, Quizzes, Feed).
    /// </summary>
    Task<List<Course>> GetByLecturerUuidAsyncFull(Guid lecturerUuid, CancellationToken ct = default);
    
    Task CreateAsync(Course course, CancellationToken ct = default);
    
    Task UpdateAsync(Course course, CancellationToken ct = default);
    
    Task<bool> DeleteAsync(Guid uuid, CancellationToken ct = default);
}