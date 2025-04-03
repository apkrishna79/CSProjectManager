using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CS_Project_Manager.Models
{
    public class UserAvailability
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        public required string Day { get; set; }
        public required string Time { get; set; }

        [BsonElement("AssocUserId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocUserId { get; set; }

        [BsonElement("AssocTeamId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocTeamId { get; set; }
    }
}