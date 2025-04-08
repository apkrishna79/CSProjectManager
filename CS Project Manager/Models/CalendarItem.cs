/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Ginny Ke - added comments
Date Revised: 4/4/25
Purpose: model to handle calendar items in databse
*/
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models 
{
    public class CalendarItem // creates calendaritem class that passes in object id, event name, state date time, end date time, assoc team id, and notes. 
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
        public string? Notes { get; set; }
    }
}
