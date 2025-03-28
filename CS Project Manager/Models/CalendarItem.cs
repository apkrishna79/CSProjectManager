using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class CalendarItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [Required]
        public required string EventName { get; set; }
        [Required]
        public DateTime StartDateTime { get; set; }
        [Required]
        public DateTime EndDateTime { get; set; }
        [BsonElement("AssocTeamId")]
        [BsonRepresentation(BsonType.ObjectId)] 
        public ObjectId AssocTeamId { get; set; }
        [Required]
        public required Dictionary<ObjectId, string> Attendees { get; set; } // Yes No Maybe
        public string? Notes { get; set; }
    }
}