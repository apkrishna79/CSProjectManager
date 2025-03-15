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
        [BsonElement("TodoID")]
        public bool is_team_item { get; set; }
        [Required]
        public required string item_name { get; set; }
        public bool item_complete { get; set; }
        [BsonElement("AssocUserId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocUserId { get; set; }

        [BsonElement("AssocTeamId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocTeamId { get; set; }
    }

}