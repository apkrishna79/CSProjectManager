using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

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

        public string? Title { get; set; }

        public required string Content;

        public DateTime Timestamp;

        [BsonElement("HeadPostId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId HeadPostId { get; set; }

        [BsonElement("ParentPostId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId ImmediateParentPostId { get; set; }

        public bool IsReply { get; set; }
    }
}
