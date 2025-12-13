using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tda26.Server.Data.Models;

public class FeedPost : Auditable {
    [Key] public Guid Uuid { get; set; } = Guid.NewGuid();

    public Guid CourseUuid { get; set; }
    [ForeignKey("CourseUuid")]
    public Course Course { get; set; } = null!;
}