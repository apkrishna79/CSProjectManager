/*
 * Prologue
Created By: Dylan Sailors
Date Created: 3/1/25
Last Revised By: Dylan Sailors
Date Revised: 3/9/25 DS
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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CS_Project_Manager.Pages
{
    public class RequirementsStackModel(ProjectService projectService, TeamService teamService, StudentUserService studentUserService) : PageModel
    {
        private readonly RequirementService _requirementService;
        private readonly ProjectService _projectService;

        [BindProperty]
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();

        [BindProperty]
        public Requirement NewRequirement { get; set; } = new Requirement
        {
            Description = string.Empty
        };

        [BindProperty(SupportsGet = true)]
        public ObjectId ProjectId { get; set; }

        public RequirementsStackModel(RequirementService requirementService, ProjectService projectService)
        {
            _requirementService = requirementService;
            _projectService = projectService;
        }

        public async Task OnGetAsync(string projectId)
        {
            if (!ObjectId.TryParse(projectId, out ObjectId parsedProjectId))
            {
                throw new ArgumentException("Invalid project ID format");
            }
            ProjectId = parsedProjectId;
            Requirements = await _requirementService.GetRequirementsByProjectIdAsync(parsedProjectId);
        }

        // Add a new requirement
        public async Task<IActionResult> OnPostAddAsync(ObjectId projectId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            ProjectId = projectId;
            var project = await _projectService.GetProjectById(ProjectId);
            if (project == null)
            {
                return NotFound();
            }
            NewRequirement.AssocProjectId = ProjectId;
            await _requirementService.AddRequirementAsync(NewRequirement);
            return RedirectToPage(new { projectId = ProjectId.ToString() });
        }

        // Update an existing requirement
        public async Task<IActionResult> OnPostUpdateAsync(ObjectId id, ObjectId projectId)
        {
            var existingRequirement = await _requirementService.GetRequirementByIdAsync(id);
            if (existingRequirement == null)
            {
                return NotFound();
            }
            var updatedRequirement = Requirements.FirstOrDefault(r => r.Id == id);
            if (updatedRequirement != null)
            {
                existingRequirement.RequirementID = updatedRequirement.RequirementID;
                existingRequirement.Description = updatedRequirement.Description;
                existingRequirement.StoryPoints = updatedRequirement.StoryPoints;
                existingRequirement.Priority = updatedRequirement.Priority;
                existingRequirement.SprintNo = updatedRequirement.SprintNo;
                await _requirementService.UpdateRequirementAsync(existingRequirement);
            }
            return RedirectToPage(new { projectId = projectId.ToString() });
        }

        // Remove an existing requirement
        public async Task<IActionResult> OnPostRemoveAsync(ObjectId id, ObjectId projectId)
        {
            var requirement = await _requirementService.GetRequirementByIdAsync(id);
            if (requirement == null)
            {
                return NotFound();
            }
            await _requirementService.RemoveRequirementAsync(id);
            return RedirectToPage(new { projectId = projectId.ToString() });
        }

    }
}
