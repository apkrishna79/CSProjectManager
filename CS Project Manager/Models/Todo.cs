using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class Todo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public bool IsTeamItem { get; set; }
        [Required]
        public required string ItemName { get; set; }
        public bool ItemComplete { get; set; }
        [BsonElement("AssocUserId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocUserId { get; set; }

        [BsonElement("AssocTeamId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocTeamId { get; set; }
    }

}