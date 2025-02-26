/*
Name: Isabel Loney
Date Created: 2/24/25
Date Revised: 2/25/25
Purpose: StudentUser model for MongoDB storage.
*/

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;


namespace CS_Project_Manager.Models
{
    public class StudentUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Username may not exceed 100 characters")]
        public required string Username { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        [MaxLength(100, ErrorMessage = "First name may not exceed 100 characters")]
        public required string FirstName { get; set; }

        [MaxLength(100, ErrorMessage = "Last name may not exceed 100 characters")]
        public required string LastName { get; set; }

        [EmailAddress]
        [MaxLength(255, ErrorMessage = "Email may not exceed 255 characters")]
        public string? Email { get; set; }
    }

}
