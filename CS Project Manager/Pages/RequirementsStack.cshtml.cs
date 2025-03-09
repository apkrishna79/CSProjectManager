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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CS_Project_Manager.Pages
{
    public class RequirementsStackModel : PageModel
    {
        private readonly RequirementService _requirementService;
        private readonly ProjectService _projectService;

        [BindProperty]
        public Requirement NewRequirement { get; set; } = new Requirement
        {
            Description = string.Empty
        };

        public List<Requirement> Requirements { get; set; } = new();

        public ObjectId ProjectId { get; set; }

        public RequirementsStackModel(RequirementService requirementService, ProjectService projectService)
        {
            _requirementService = requirementService;
            _projectService = projectService;
        }

        // Runs when the page is accessed (fetches requirements for a specific project)
        public async Task<IActionResult> OnGetAsync(string projectId)
        {
            if (!ObjectId.TryParse(projectId, out ObjectId parsedProjectId))
            {
                return BadRequest("Invalid project ID.");
            }

            ProjectId = parsedProjectId;
            Requirements = await _requirementService.GetRequirementsByProjectIdAsync(ProjectId);

            return Page();
        }

        // Handler for adding a new requirement
        public async Task<IActionResult> OnPostAddAsync(string projectId)
        {
            if (!ObjectId.TryParse(projectId, out ObjectId parsedProjectId))
            {
                return BadRequest("Invalid project ID.");
            }

            NewRequirement.AssocProjectId = parsedProjectId;
            await _requirementService.AddRequirementAsync(NewRequirement);

            return RedirectToPage(new { projectId });
        }

        // Handler for updating an existing requirement
        public async Task<IActionResult> OnPostUpdateAsync(ObjectId id)
        {
            var requirementToUpdate = await _requirementService.GetRequirementByIdAsync(id);
            if (requirementToUpdate != null)
            {
                // Update logic (e.g., fetch new values from a form if needed)
                await _requirementService.UpdateRequirementAsync(requirementToUpdate);
            }

            return RedirectToPage(new { projectId = requirementToUpdate.AssocProjectId.ToString() });
        }

        // Handler for removing an existing requirement
        public async Task<IActionResult> OnPostRemoveAsync(ObjectId id)
        {
            await _requirementService.RemoveRequirementAsync(id);

            return RedirectToPage(new { projectId = NewRequirement.AssocProjectId.ToString() });
        }
    }
}
