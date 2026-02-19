using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using tda26.Server.DTOs;
using tda26.Server.DTOs.Converters;
using tda26.Server.DTOs.Mapping;


namespace tda26.Server.Data.Models;


[Table(name: "FeedPosts")]
public sealed class FeedPost : Auditable {

    public const ushort MESSAGE_MAX_LENGTH = 2048;



    // mapovani props na sloupce v db
    [Key]
    public required Guid Uuid { get; set; } = Guid.NewGuid();

    [Required]
    public required FeedPostType Type { get; set; }

    [JsonIgnore]
    public Guid CourseUuid { get; set; }

    [ForeignKey(nameof(CourseUuid)), JsonIgnore]
    public Course Course { get; set; } = null!;

    [JsonIgnore]
    public Guid? AccountUuid { get; set; } = null;

    [ForeignKey(nameof(AccountUuid)), JsonIgnore]
    public Account? Account { get; set; } = null!;

    [Required, MaxLength(MESSAGE_MAX_LENGTH)]
    public required string Message { get; set; }

    public bool Edited { get; set; } = false;
    
    public FeedPurpose Purpose { get; set; } = FeedPurpose.Default;
    
    
    // nemapovany props (pouze pro serializaci)
    [JsonConverter(typeof(JsonStringEnumLowerCaseConverter))]
    public enum FeedPostType { Manual, System }
    
    [JsonConverter(typeof(JsonStringEnumCamelCaseConverter))]
    public enum FeedPurpose { CreateMaterial, UpdateMaterial, DeleteMaterial, CreateQuiz , UpdateQuiz, DeleteQuiz , Default }

    [NotMapped]
    public AuthorDto? Author => Account?.ToAuthorDto(); // pokud je author null, tak se pravdepodobne jedna o systemovy prispevek
}