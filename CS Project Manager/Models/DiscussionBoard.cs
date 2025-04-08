/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Ginny Ke - added comments
Date Revised: 4/4/25
Purpose: model to handle the discussion board in the database
*/
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CS_Project_Manager.Models
{
    public class DiscussionBoard // creates discussion board class that passes in objectid, isclassboard, class id, and teamid. 
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
