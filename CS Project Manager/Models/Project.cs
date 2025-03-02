using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class Project
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Project name cannot exceed 100 characters")]
        public required string project_name { get; set; }

        [Required]
        public required string description { get; set; }

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId AssociatedTeam { get; set; }
    }
}