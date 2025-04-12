/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Anakha Krishna - comments
Date Revised: 4/12/25
Purpose: model to handle classes in database
*/
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Models
{
    public class Class // creates public class that passes in object id, name, and enrolled students. 
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } // ObjectId identifier in Mongo

        [Required]
        [MaxLength(100, ErrorMessage = "Class name cannot exceed 100 characters")]
        public required string Name { get; set; } // Name of class

        public List<ObjectId> EnrolledStudents { get; set;  } = []; // Array of student ObjectIds enrolled in class
    }
}
