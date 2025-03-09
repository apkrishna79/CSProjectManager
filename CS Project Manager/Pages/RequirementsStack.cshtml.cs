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
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CS_Project_Manager.Pages
{
    public class RequirementsStackModel : PageModel
    {
        private readonly RequirementService _requirementService;

        public RequirementsStackModel(RequirementService requirementService)
        {
            _requirementService = requirementService;
            Requirements = new List<Requirement>();
        }

        [BindProperty]
        public Requirement NewRequirement { get; set; } = new Requirement
        {
            Description = string.Empty
        };

        public List<Requirement> Requirements { get; set; }

        public async Task OnGetAsync(string projectId)
        {
            if (!ObjectId.TryParse(projectId, out ObjectId projectObjectId))
            {
                Requirements = new List<Requirement>();
                return;
            }

            Requirements = await _requirementService.GetRequirementsByProjectIdAsync(projectObjectId);
        }



        public async Task<IActionResult> OnPostAddAsync(string projectId)
        {
            if (!ObjectId.TryParse(projectId, out ObjectId projectObjectId))
            {
                return BadRequest("Invalid project ID");
            }

            if (!ModelState.IsValid)
            {
                await OnGetAsync(projectId);  // Ensure the page reloads with the correct project data
                return Page();
            }

            // Associate the new requirement with the project
            NewRequirement.AssocProjectId = projectObjectId;

            await _requirementService.CreateRequirementAsync(NewRequirement);
            return RedirectToPage(new { projectId = projectId }); // Redirect with the projectId to keep context
        }


        public async Task<IActionResult> OnPostUpdateAsync(string Id, int RequirementID, string Description, int StoryPoints, int Priority, int SprintNo)
        {
            if (!ObjectId.TryParse(Id, out ObjectId objectId))
            {
                return BadRequest("Invalid requirement ID");
            }

            var updatedRequirement = new Requirement
            {
                Id = objectId,
                RequirementID = RequirementID,
                Description = Description,
                StoryPoints = StoryPoints,
                Priority = Priority,
                SprintNo = SprintNo
            };

            await _requirementService.UpdateRequirementAsync(objectId, updatedRequirement);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAsync(string Id)
        {
            if (!ObjectId.TryParse(Id, out ObjectId objectId))
            {
                return BadRequest("Invalid requirement ID");
            }

            await _requirementService.DeleteRequirementAsync(objectId);
            return RedirectToPage();
        }

    }
}
