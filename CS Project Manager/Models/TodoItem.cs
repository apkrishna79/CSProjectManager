/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Dylan Sailors - support for tags
Date Revised: 4/27/25
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
        public ObjectId Id { get; set; } // ObjectId identifier in Mongo
        public bool IsTeamItem { get; set; } // IsTeamItem == true shows up on team to do list, IsTeamItem == false means it is a personal item
        [Required]
        public required string ItemName { get; set; } // Name of item
        public bool ItemComplete { get; set; } // Keep track of item completion
        [BsonElement("AssocUserId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocUserId { get; set; } // User that created the item if personal item

        [BsonElement("AssocTeamId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocTeamId { get; set; } // Team associated with item if team item
        public string Tag { get; set; } = "No tag"; // Tag associated with todo item
    }

}
