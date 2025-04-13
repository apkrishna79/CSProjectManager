/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Anakha Krishna - comments
Date Revised: 4/12/25
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
        public ObjectId Id { get; set; } // ObjectId identifier in Mongo

        [BsonElement("Description")]
        [Required]
        public required string Description { get; set; } // Text description of the BrainstormItem

        [BsonElement("CreatedBy")]
        [BsonRequired]
        public ObjectId CreatedBy { get; set; } // ObjectId of user who created the BrainstormItem

        [BsonElement("Upvotes")]
        public List<ObjectId> Upvotes { get; set; } = []; // Array of user ObjectIds of those who upvote the BrainstormItem

        [BsonElement("Downvotes")]
        public List<ObjectId> Downvotes { get; set; } = []; // Array of user ObjectIds of those who downvote the BrainstormItem

        [BsonElement("AssocProjectId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AssocProjectId { get; set; } // Associated project ObjectId
    }
}
