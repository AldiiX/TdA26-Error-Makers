using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BCrypt.Net;

namespace tda26.Server.Infrastructure;





public static class Utilities {

    private const HashType EnhancedType = HashType.SHA384;



    public static void SetObject<T>(this ISession session, in string key, in T value) {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetObject<T>(this ISession session, in string key) where T : class? {
        var value = session.GetString(key);
        return value == null ? null : JsonSerializer.Deserialize<T>(value);
    }

    public static T? GetObject<T>(this ISession session, string key) where T : struct {
        var value = session.GetString(key);
        return value == null ? (T?)null : JsonSerializer.Deserialize<T>(value);
    }

    public static V? GetValueOrNull<K, V>(this IDictionary<K, V> dictionary, K key) {
        return dictionary.TryGetValue(key, out var value) ? value : default;
    }

    public static string GenerateRandomPassword(int length = 24) {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789ěščřž!@*";
        var random = new Random();
        var passwordBuilder = new StringBuilder(length);

        for (int i = 0; i < length; i++) {
            var randomIndex = random.Next(chars.Length);
            passwordBuilder.Append(chars[randomIndex]);
        }

        return passwordBuilder.ToString();
    }

    public static string EncryptPassword(string plain) => HashPassword(plain);

    public static string HashPassword(string plain, int workFactor = 12) {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(plain, workFactor, EnhancedType);
    }

    public static bool VerifyPassword(in string password, in string hashedPassword) {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword)) return false;
        try {
            // nejdriv enhanced, pak klasicky
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword, EnhancedType)
                   || BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        } catch (BCrypt.Net.SaltParseException) {
            Program.Logger.LogError("SaltParseException in Utilities.VerifyPassword");
            return false;
        }
    }

    public static bool IsPasswordValid(in string password) {
        if (string.IsNullOrEmpty(password)) return false;

        if (password.Length < 8) return false;

        if (!password.Any(char.IsUpper)) return false;

        if (!password.Any(char.IsLower)) return false;

        if (!password.Any(char.IsDigit)) return false;

        if (!password.Any(c => "ěščřžýáíéůú!@#$%^&*()_+-=[]{}|;':\",.<>?/".Contains(c))) return false;

        return true;
    }

    public static string ToJsonString(this object obj) {
        return JsonSerializer.Serialize(obj, JsonSerializerOptions.Web);
    }

    public static JsonNode ToJsonNode(this object obj, in JsonSerializerOptions? options = null) {
        return JsonSerializer.SerializeToNode(obj, options ?? JsonSerializerOptions.Web) ?? new JsonObject();
    }
    
    public static bool IsAllowedMimeType(this IFormFile file) {
        var allowedMimeTypes = new List<string> {
            // Documents
            "application/pdf", // .pdf
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
            "text/plain", // .txt

            // Images
            "image/png", // .png
            "image/jpeg", // .jpg, .jpeg
            "image/gif", // .gif

            // Media
            "video/mp4", // .mp4
            "audio/mpeg" // .mp3
        };
            
        var fileMimeType = file.ContentType.ToLowerInvariant().Split(';')[0];

        return allowedMimeTypes.Contains(fileMimeType);
    }
    
    public static bool IsAllowedFileSize(this IFormFile file) {
        const long maxFileSizeBytes = 30 * 1024 * 1024;
        return !(file.Length > maxFileSizeBytes);
    }
}