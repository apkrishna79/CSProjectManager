/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Anakha Krishna - comments
Date Revised: 4/12/25
Purpose: model to handle discussion posts in database
*/

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models 
{
    public class DiscussionPost // creates class that passes in object id, boardid, created by, title, timestamp, content, headpostid, is reply, and replyIds. 
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } // ObjectId identifier in Mongo

        [BsonElement("BoardId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId BoardId { get; set; } // Associated boardId the post is posted on

        [BsonElement("CreatedBy")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId CreatedBy { get; set; } // ObjectId of student that created the post

        [MaxLength(100, ErrorMessage = "Title may not exceed 100 characters")]
        public string? Title { get; set; } // Title of post

        public required string Content; // Content of post

        public DateTime Timestamp; // Timestamp of when post is created

        [BsonElement("HeadPostId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId HeadPostId { get; set; } // Id of parent post

        public bool IsReply { get; set; } // boolean to keep track of whether a post is a  reply or not

        public List<ObjectId> ReplyIds { get; set; } = []; // Array of postIds to keep track of replies
    }
}
