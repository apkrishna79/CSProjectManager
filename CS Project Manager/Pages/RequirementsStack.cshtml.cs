/*
 * Prologue
Created By: Dylan Sailors
Date Created: 3/1/25
Last Revised By: Dylan Sailors
Date Revised: 3/2/25
Purpose: Let users add/update/remove requirements to a blank requirements stack generated once a project is created
Preconditions: MongoDBService, ProjectService instances properly initialized and injected; Requirement must be correctly defined
Postconditions: Users can add, update, and remove project requirements
Error and exceptions: ArgumentNullException (required field empty), FormatException (invalid data input)
Side effects: N/A
Invariants: Requirements list is always initialized; OnGet method always prepares an initial list
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace CS_Project_Manager.Pages
{
    public class RequirementsStackModel : PageModel
    {
        private readonly ProjectService _projectService;

        [BindProperty]
        public string ProjectId { get; set; }

        [BindProperty]
        public Project? CurrentProject { get; set; }

        public List<Requirement> Requirements { get; set; } = new List<Requirement>();

        public RequirementsStackModel(ProjectService projectService)
        {
            _projectService = projectService;
        }

        // Handles the GET request to load the project and its requirements
        public async Task<IActionResult> OnGetAsync(string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return NotFound();
            }
            ProjectId = projectId;
            CurrentProject = await _projectService.GetProjectById(new ObjectId(ProjectId));
            if (CurrentProject == null)
            {
                return NotFound();
            }
            Requirements = CurrentProject.Requirements ?? new List<Requirement>();
            return Page();
        }

        // Handles POST request to add a new requirement to the project
        public async Task<IActionResult> OnPostAddRequirementAsync(Requirement newRequirement)
        {
            if (string.IsNullOrEmpty(ProjectId))
            {
                return NotFound();
            }
            await _projectService.AddRequirementAsync(new ObjectId(ProjectId), newRequirement);
            return RedirectToPage(new { projectId = ProjectId });
        }

        // Handles POST request to remove an existing requirement from the project
        public async Task<IActionResult> OnPostRemoveRequirementAsync(string requirementId)
        {
            if (string.IsNullOrEmpty(ProjectId))
            {
                return NotFound();
            }
            await _projectService.RemoveRequirementAsync(new ObjectId(ProjectId), requirementId);
            return RedirectToPage(new { projectId = ProjectId });
        }

        // Handles POST request to update an existing requirement in the project
        public async Task<IActionResult> OnPostUpdateRequirementAsync(Requirement requirement)
        {
            if (string.IsNullOrEmpty(ProjectId))
            {
                return NotFound();
            }
            var project = await _projectService.GetProjectById(new ObjectId(ProjectId));
            if (project == null)
            {
                return NotFound();
            }
            var existingRequirement = project.Requirements.FirstOrDefault(r => r.RequirementID == requirement.RequirementID);
            if (existingRequirement != null)
            {
                existingRequirement.Description = requirement.Description;
                existingRequirement.StoryPoints = requirement.StoryPoints;
                existingRequirement.Priority = requirement.Priority;
                existingRequirement.SprintNo = requirement.SprintNo;

                await _projectService.UpdateProjectAsync(project);
            }
            return RedirectToPage(new { projectId = ProjectId });
        }
    }
}
