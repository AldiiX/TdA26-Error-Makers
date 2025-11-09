using MySqlConnector;
using tda26.Server.Classes.Objects;
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

    public async Task<bool> CreateAsync(Course course, CancellationToken ct = default)
    {
        await using var conn = await db.GetOpenConnectionAsync(ct);
        if (conn == null)
            return false;
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
        ";
        cmd.Parameters.AddWithValue("@uuid", course.Uuid);
        cmd.Parameters.AddWithValue("@name", course.Name);
        cmd.Parameters.AddWithValue("@description", course.Description);
        cmd.Parameters.AddWithValue("@created_at", course.CreatedAt);
        cmd.Parameters.AddWithValue("@updated_at", course.UpdatedAt);
        var result = await cmd.ExecuteNonQueryAsync(ct);
        return result > 0;
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
            reader.GetDateTime("updated_at")
        );
    }
    
}