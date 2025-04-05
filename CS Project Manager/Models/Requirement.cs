/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Ginny Ke - added comments
Date Revised: 4/4/25
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
        public ObjectId Id { get; set; }
        [BsonElement("RequirementID")]
        public int? RequirementID { get; set; }
        [Required]
        public required string Description { get; set; }
        public int? StoryPoints { get; set; }
        public int? Priority { get; set; }
        public int? SprintNo { get; set; }
        [BsonElement("AssocProjectId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocProjectId { get; set; }
        public List<ObjectId> Assignees { get; set; } = [];
        public bool IsComplete { get; set; } 
    }
}
