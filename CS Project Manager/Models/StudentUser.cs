/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Anakha Krishna - comments
Date Revised: 4/12/25
Purpose: model to handle student users in database
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;


namespace CS_Project_Manager.Models
{
    public class StudentUser // Student User class that passes in objectid, username, password, first/last name, and email.
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; } // ObjectId identifier in Mongo

        [Required]
        [EmailAddress]
        [MaxLength(255, ErrorMessage = "Email may not exceed 255 characters")]
        public required string Email { get; set; } // Email, must be KU email handled by Regex, no duplicate emails
        [Required]
        public required string PasswordHash { get; set; } // Hashed password

        [Required]
        [MaxLength(100, ErrorMessage = "First name may not exceed 100 characters")]
        public required string FirstName { get; set; } // First name of Student user

        [Required]
        [MaxLength(100, ErrorMessage = "Last name may not exceed 100 characters")]
        public required string LastName { get; set; } // Last name of student user

        
    }

}
