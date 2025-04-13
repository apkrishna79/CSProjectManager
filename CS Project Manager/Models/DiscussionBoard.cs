/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Anakha Krishna - comments
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
        public ObjectId Id { get; set; } // ObjectId identifier in Mongo

        public bool IsClassBoard; // Boolean to indicate whether a board is a classboard or not. If false, it's a team discussion board

        [BsonElement("ClassId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId ClassId { get; set; } // Associated class ObjectId with discussion board if IsClassBoard == true, else ObjectId.empty

        [BsonElement("TeamId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId TeamId { get; set; } // ASsociated team ObjectId with discussionboard if IsClassBoard == false, else ObjectId.empty
    }
}
