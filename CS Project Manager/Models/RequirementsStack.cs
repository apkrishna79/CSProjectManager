using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class Requirement
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("RequirementID")]
        public int RequirementID { get; set; }

        [Required]
        public required string Description { get; set; }

        public int StoryPoints { get; set; }
        public int Priority { get; set; }
        public int SprintNo { get; set; }

        [BsonElement("AssocProjectId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocProjectId { get; set; }
    }
}
