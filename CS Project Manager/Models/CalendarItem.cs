/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Anakha Krishna - comments
Date Revised: 4/12/25
Purpose: model to handle calendar items in database
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
        public ObjectId Id { get; set; } // ObjectId identifier in Mongo

        [Required]
        public required string EventName { get; set; } // Name describing the event
        [Required]
        public DateTime StartDateTime { get; set; } // Start date of event
        [Required]
        public DateTime EndDateTime { get; set; } // End date of event
        [BsonElement("AssocTeamId")]
        [BsonRepresentation(BsonType.ObjectId)] 
        public ObjectId AssocTeamId { get; set; } // Associated team ObjectId with event
        public string? Notes { get; set; }
    }
}
