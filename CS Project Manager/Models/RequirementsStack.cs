using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using static System.Net.WebRequestMethods;

namespace CS_Project_Manager.Models
{
    public class Requirement
    {
        [Required]
        public required string RequirementID { get; set; } 

        [Required]
        public required string Description { get; set; } 

        [Range(1, 100)]
        public int StoryPoints { get; set; }

        [Required]
        public required string Priority { get; set; } 

        [Range(1, int.MaxValue, ErrorMessage = "Sprint number must be at least 1")]
        public int SprintNo { get; set; }
    }
}