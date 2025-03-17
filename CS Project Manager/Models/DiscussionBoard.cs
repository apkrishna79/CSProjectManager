using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CS_Project_Manager.Models
{
    public class DiscussionBoard
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        public bool IsClassBoard;

        // null if team board
        [BsonElement("ClassId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId ClassId { get; set; }

        // null if class board
        [BsonElement("TeamId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId TeamId { get; set; }
    }
}
