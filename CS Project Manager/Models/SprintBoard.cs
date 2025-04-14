/*
* Prologue
Created By: Ginny Ke
Date Created: 4/12/25
Last Revised By: Ginny Ke
Date Revised: 4/13/25
Purpose: Model to handle sprint retrospective board in database
*/
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class SprintBoardItem // class that passes in category, description, objectid, sprint number, created by, and project id.
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("Category")]
        [Required]
        public required string Category { get; set; }

        [BsonElement("Description")]
        [Required]
        public required string Description { get; set; }

        [BsonElement("SprintNumber")]
        [Required]
        public int SprintNumber { get; set; }

        [BsonElement("CreatedBy")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId CreatedBy { get; set; }

        [BsonElement("AssocProjectId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocProjectId { get; set; }
    }
}