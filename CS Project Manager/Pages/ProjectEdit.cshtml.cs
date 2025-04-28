/*
* Prologue
Created By: Jackson Wunderlich
Date Created: 4/27/25
Last Revised By: Jackson Wunderlich
Date Revised: 4/27/25
Purpose: Handles the logic for editing project details
Preconditions: User must be logged in, ProjectService must be available
Postconditions: Project details are updated
Error and exceptions: ArgumentNullException, MongoException
Side effects: N/A
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Pages
{
    public class ProjectEditModel(StudentUserService studentUserService, ProjectService projectService) : PageModel
    {
        private readonly StudentUserService _studentUserService = studentUserService;
        private readonly ProjectService _projectService = projectService;

        // Properties for project data
        [BindProperty]
        [Required]
        [MaxLength(255)]
        public string ProjectName { get; set; }

        [BindProperty]
        [Required]
        [MaxLength(255)]
        public string ProjectDescription { get; set; }

        [BindProperty]
        public ObjectId ProjectId { get; set; }

        public string ErrorMessage { get; set; }

        // loads project data
        public async Task OnGetAsync(ObjectId projectId)
        {
            var currentProject = await _projectService.GetProjectById(projectId);
            ProjectId = projectId;
            ProjectName = currentProject.ProjectName;
            ProjectDescription = currentProject.Description;
        }

        // changes the name of the project on form submission
        public async Task<IActionResult> OnPostUpdateNameAsync(ObjectId projectId)
        {
            // check for existing project
            var existingProject = await _projectService.GetProjectById(ProjectId);
            if (existingProject == null)
            {
                return NotFound();
            }
            existingProject.ProjectName = ProjectName;
            // update project in the database
            await _projectService.UpdateProjectAsync(existingProject);
            await OnGetAsync(existingProject.Id);

            return RedirectToPage(new { projectId });
        }

        // changes the description of the project on form submission
        public async Task<IActionResult> OnPostUpdateDescriptionAsync(ObjectId projectId)
        {
            // check for existing project
            var existingProject = await _projectService.GetProjectById(ProjectId);
            if (existingProject == null)
            {
                return NotFound();
            }
            existingProject.Description = ProjectDescription;
            // update project in the database
            await _projectService.UpdateProjectAsync(existingProject);
            await OnGetAsync(existingProject.Id);

            return RedirectToPage(new { projectId });
        }
    }
}
