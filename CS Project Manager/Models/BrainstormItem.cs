/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Ginny Ke - added comments
Date Revised: 4/4/25
Purpose: model to handle brainstorm item in the database
*/
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class BrainstormItem // creates brainstorm item class that passes in object id, description, createdby, upvotes, downvotes, and assoc project id. 
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("Description")]
        [Required]
        public required string Description { get; set; }

        [BsonElement("CreatedBy")]
        [BsonRequired]
        public ObjectId CreatedBy { get; set; }

        [BsonElement("Upvotes")]
        public List<ObjectId> Upvotes { get; set; } = [];

        [BsonElement("Downvotes")]
        public List<ObjectId> Downvotes { get; set; } = [];

        [BsonElement("AssocProjectId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocProjectId { get; set; }
    }
}
