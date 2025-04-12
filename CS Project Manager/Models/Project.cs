/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Anakha Krishna - comments
Date Revised: 4/12/25
Purpose: model to handle projects
*/
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CS_Project_Manager.Models
{
    public class Project // creates project class that passes in project name, objectid, description, associated team, and requirements. 
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } // ObjectId identifier in Mongo

        [Required]
        [MaxLength(100, ErrorMessage = "Project name cannot exceed 100 characters")]
        public required string ProjectName { get; set; } // Name of project

        [Required]
        public required string Description { get; set; } // Project description

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        public required ObjectId AssociatedTeam { get; set; } // Team ObjectId associated with project

        public List<Requirement> Requirements { get; set; } = new(); // List of requirements

    }
}
