/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Ginny Ke - added comments
Date Revised: 4/4/25
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
        public ObjectId Id { get; set; }

        [BsonElement("BoardId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId BoardId { get; set; }

        [BsonElement("CreatedBy")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId CreatedBy { get; set; }

        [MaxLength(100, ErrorMessage = "Title may not exceed 100 characters")]
        public string? Title { get; set; }

        public required string Content;

        public DateTime Timestamp;

        [BsonElement("HeadPostId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId HeadPostId { get; set; }

        public bool IsReply { get; set; }

        public List<ObjectId> ReplyIds { get; set; } = [];
    }
}
