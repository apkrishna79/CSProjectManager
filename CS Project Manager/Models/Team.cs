/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Ginny Ke - added comments
Date Revised: 4/4/25
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
        public ObjectId Id { get; set; }

        [MaxLength(100, ErrorMessage = "Team name cannot exceed 100 characters")]
        public required string Name { get; set; }

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId AssociatedClass { get; set; }

        public List<ObjectId> Members = [];
    }
}
