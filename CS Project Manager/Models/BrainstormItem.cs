using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class BrainstormItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("Description")]
        [Required]
        public required string Description { get; set; }

        [BsonElement("CreatedBy")]
        [BsonRequired]
        public ObjectId CreatedBy { get; set; }

        [BsonElement("Upvotes")]
        public List<ObjectId> Upvotes { get; set; } = [];

        [BsonElement("Downvotes")]
        public List<ObjectId> Downvotes { get; set; } = [];

        [BsonElement("AssocProjectId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocProjectId { get; set; }
    }
}
