/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Anakha Krishna - comments
Date Revised: 4/12/25
Purpose: model to handle teams in database
*/
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class Team // create team class that passes in objectId, associated class, and members
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } // ObjectId identifier in Mongo

        [MaxLength(100, ErrorMessage = "Team name cannot exceed 100 characters")]
        public required string Name { get; set; } // Name of team

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId AssociatedClass { get; set; } // Class the team is a part of, Class ObjectId

        public List<ObjectId> Members = []; // Members of team, StudentUser ObjectIds
    }
}
