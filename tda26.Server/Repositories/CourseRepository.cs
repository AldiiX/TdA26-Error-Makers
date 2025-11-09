using MySqlConnector;
using tda26.Server.Classes.Objects;
using tda26.Server.Infrastructure;
using tda26.Server.Services;

namespace tda26.Server.Repositories;

public class CourseRepository(IDatabaseService db) : ICourseRepository 
{
    public async Task<Course?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        await using var conn = await db.GetOpenConnectionAsync(ct);
        if (conn == null)
            return null;
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                uuid,
                name,
                description,
                created_at,
                updated_at
            FROM courses
            WHERE uuid = @uuid;
        ";
        cmd.Parameters.AddWithValue("@uuid", uuid);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;
        var course = MapCourseFromReader(reader);
        return course;
    }

    public async Task<List<Course>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = await db.GetOpenConnectionAsync(ct);
        if (conn == null)
            return new List<Course>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"

            SELECT 
                uuid,
                name,
                description,
                created_at,
                updated_at
            FROM courses;
        ";
        var courses = new List<Course>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var course = MapCourseFromReader(reader);
            courses.Add(course);
        }
        return courses;
    }

    public async Task<Course?> CreateAsync(string name, string description, CancellationToken ct = default)
    {
        await using var conn = await db.GetOpenConnectionAsync(ct);
        if (conn == null)
            return null;
        
        
        
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO courses (
                uuid,
                name,
                description,
                created_at,
                updated_at
            ) VALUES (
                @uuid,
                @name,
                @description,
                @created_at,
                @updated_at
            );
            SELECT * FROM courses WHERE uuid = @uuid;
        ";
        cmd.Parameters.AddWithValue("@uuid", Guid.NewGuid());
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@description", description);
        cmd.Parameters.AddWithValue("@created_at", DateTime.Now);
        cmd.Parameters.AddWithValue("@updated_at", DateTime.Now);
        var result = await cmd.ExecuteReaderAsync(ct);
        if (!await result.ReadAsync(ct)) return null;
        var course = MapCourseFromReader(result);
        return course;
    }

    public async Task<bool> UpdateAsync(Course course, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
    
    
    
    // mapovani objektu z readeru 
    
    public Course MapCourseFromReader(MySqlDataReader reader)
    {
        return new Course(
            reader.GetGuid("uuid"),
            reader.GetString("name"),
            reader.GetString("description"),
            reader.GetDateTime("created_at"),
            reader.GetDateTime("updated_at"),
            reader.GetStringOrNull("image_url")
        );
    }
    
}