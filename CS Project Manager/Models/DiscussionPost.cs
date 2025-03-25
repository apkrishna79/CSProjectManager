using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class DiscussionPost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("BoardId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId BoardId { get; set; }

        [BsonElement("PosterId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PosterId { get; set; }

        [MaxLength(100, ErrorMessage = "Title may not exceed 100 characters")]
        public string? Title { get; set; }

        public required string Content;

        public DateTime Timestamp;

        [BsonElement("HeadPostId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId HeadPostId { get; set; }

        public bool IsReply { get; set; }

        public List<ObjectId> ReplyIds { get; set; } = [];
    }
}
