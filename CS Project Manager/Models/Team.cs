using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class Team
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [MaxLength(100, ErrorMessage = "Team name cannot exceed 100 characters")]
        public required string Name { get; set; }

        [Required]
        public required string AssociatedClass { get; set; }

        public List<ObjectId> Members = [];
    }
}
