/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Ginny Ke - added comments
Date Revised: 4/4/25
Purpose: model to handle todo list items 
*/
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class TodoItem // create class that passes in objectid, isTeamItem, ItemName, ItemComplete, UserId, and TeamID to the database
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        public bool IsTeamItem { get; set; }
        [Required]
        public required string ItemName { get; set; }
        public bool ItemComplete { get; set; }
        [BsonElement("AssocUserId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocUserId { get; set; }

        [BsonElement("AssocTeamId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocTeamId { get; set; }
    }

}
