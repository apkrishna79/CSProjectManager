/*
 * Prologue
Created By: Jackson Wunderlich
Date Created: 4/12/25
Last Revised By: Jackson Wunderlich
Date Revised: 4/27/25
Purpose: confirm deletion of a project from the database
Preconditions: service instances must be properly initialized and injected, models must be correctly defined
Postconditions: project is deleted or user is redirected to the project page
Error and exceptions: ArgumentNullException (thrown if the username or Password properties are null)
Side effects: N/A
Invariants: service fields are always initialized with a valid instance
Other faults: N/A
*/

using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class DeleteProjectModel : PageModel
    {
        // services used in the page
        private readonly StudentUserService _studentUserService;
        private readonly RequirementService _requirementService;
        private readonly ProjectService _projectService;
        private readonly TeamService _teamService;
        private readonly BrainstormService _brainstormService;

        // bound properties for the project id and project name inputted by the user
        [BindProperty]
        public string UserProjectName { get; set; }

        [BindProperty]
        public ObjectId ProjectId { get; set; }

        [BindProperty]
        public string GivenProjectName { get; set; }

        public DeleteProjectModel(RequirementService requirementService, ProjectService projectService, TeamService teamService, StudentUserService studentUserService, BrainstormService brainstormService)
        {
            _requirementService = requirementService;
            _projectService = projectService;
            _teamService = teamService;
            _studentUserService = studentUserService;
            _brainstormService = brainstormService;
        }

        // gets the current project and its name
        public async Task OnGetAsync(ObjectId projectId)
        {
            ProjectId = projectId;
            var GivenProject = await _projectService.GetProjectById(projectId);
            
            GivenProjectName = GivenProject.ProjectName;
        }

        // handles page submission, either deletes project and associated items or redirects to the same page
        public async Task<IActionResult> OnPostAsync()
        {
            await OnGetAsync(ProjectId);

            var GivenProject = await _projectService.GetProjectById(ProjectId);
            // checks if project name is the same as the given project to be deleted
            if (GivenProject.ProjectName == UserProjectName)
            {
                await _requirementService.RemoveRequirementsByProjectIdAsync(ProjectId);
                await _brainstormService.RemoveBrainstormItemsByProjectIdAsync(ProjectId);
                await _projectService.DeleteProjectByIdAsync(ProjectId);

                return RedirectToPage("/Dashboard");
            } else
            {
                ModelState.AddModelError(string.Empty, "Incorrect project name!");
                ModelState.Remove("GivenProjectName");
                ModelState.Remove("UserProjectName");
                return Page();
            }
        }
    }
}
