/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Ginny Ke - added comments
Date Revised: 4/4/25
Purpose: model to handle user availability for calendar and meetings
*/
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CS_Project_Manager.Models
{
    public class UserAvailability// create public class with ObjectId, day, time, userid, and project id passed in to database
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
