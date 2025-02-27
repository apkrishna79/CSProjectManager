/*
 * Prologue
Created By: Jackson Wunderlich
Date Created: 2/19/25
Last Revised By: Jackson Wunderlich
Date Revised: 2/26/25
Purpose: Handles project creation
Preconditions: MongoDBService, ProjectService instances properly initialized and injected; Project model must be correctly defined
Postconditions: new project is created and stored in the database if the inputs are valid
Error and exceptions: ArgumentNullException (required field empty)
Side effects: N/A
Invariants: _projectService field is always initialized with valid instance, OnPostAsync method always returns an IActionResult
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class ProjectModel(ProjectService projectService) : PageModel
    {
        // initializes injected service for operations using projects
        private readonly ProjectService _projectService = projectService;

        // creates bound properties for project creation form input
        [BindProperty]
        [Required]
        [MaxLength(100)]
        public required string project_name { get; set; }

        [BindProperty]
        [Required]
        public required string description { get; set; }

        [BindProperty]
        public List<string> StudentMembers { get; set; } = [];

        // runs when the create project button is pressed
        public async Task<IActionResult> OnPostAsync()
        {
            var newProject = new Project
            {
                project_name = "test",
                description = "awesome",
                StudentMembers = []
            };

            await _projectService.CreateProjectAsync(newProject);

            return RedirectToPage("/Dashboard");
        }
        
    }
}
