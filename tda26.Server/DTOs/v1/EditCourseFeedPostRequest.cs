using System.ComponentModel.DataAnnotations;
using tda26.Server.Data.Models;

namespace tda26.Server.DTOs.v1;

public class EditCourseFeedPostRequest {
        [MaxLength(FeedPost.MESSAGE_MAX_LENGTH)]
        public required string Message { get; set; }

        public bool Edited { get; set; } = true;
}
