/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Anakha Krishna - comments
Date Revised: 4/12/25
Purpose: model to handle requirements in the database
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class Requirement // creates requirements class that passes in object id, requirementid, description, priority, storypoints, sprintno, assignees, projectid, and isComplete.
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } // ObjectId identifier in Mongo
        [BsonElement("RequirementID")]
        public int? RequirementID { get; set; } // Requirement Id
        [Required]
        public required string Description { get; set; } // Description of requirement
        public int? StoryPoints { get; set; } // Story points for requirement
        public int? Priority { get; set; } // Priority of requirement
        public int? SprintNo { get; set; } // Sprint no for requirement
        [BsonElement("AssocProjectId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocProjectId { get; set; } // ObjectId of project the requirement is a part of
        public List<ObjectId> Assignees { get; set; } = []; // List of Student ObjectIds for those who are assigned to the requirement
        public bool IsComplete { get; set; } // Track completion of requirement
        public int? Progress { get; set; } // Progress percentage (0-100)
    }
}
