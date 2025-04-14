/*
* Prologue
Created By: Jackson Wunderlich
Date Created: 4/13/25
Last Revised By: ackson Wunderlich
Date Revised: 4/13/25
Purpose: model to handle minutes items in database
*/
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models 
{
    public class MeetingMinutes // creates meeting minutes class that passes in object id, notes, and associated meeting id
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } // ObjectId identifier in Mongo

        [Required]
        public required string Notes { get; set; } // Notes for the given meeting
 
        [BsonElement("AssocMeetingId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocMeetingId { get; set; } // Associated meeting ObjectId
    }
}
