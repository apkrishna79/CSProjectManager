/*
 * Prologue
Created By: Jackson Wunderlich
Date Created: 4/23/25
Last Revised By: Jackson Wunderlich
Date Revised: 4/24/25
Purpose: Allow users to navigate to different aspects of their projects (brainstorm boards, requirements stack, etc.)
Preconditions: MongoDBService, other service instances properly initialized and injected
Postconditions: Navigation to different sections of the project
Error and exceptions: ArgumentNullException (required field empty), FormatException (invalid data input)
Side effects: N/A
Invariants: _projectService and _teamService fields are always initialized with a valid instance
Other faults: N/A
*/

using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class ProjectLandingModel : PageModel
    {
        private readonly ProjectService _projectService;

        [BindProperty(SupportsGet = true)]
        public ObjectId ProjectId { get; set; }
        [BindProperty(SupportsGet = true)]
        public ObjectId AssocTeamId { get; set; }
        [BindProperty(SupportsGet = true)]
        public string ProjectName { get; set; }

        public ProjectLandingModel(ProjectService projectService)
        {
            _projectService = projectService;
        }

        public async Task OnGetAsync(string projectId)
        {
            if (!ObjectId.TryParse(projectId, out ObjectId parsedProjectId))
            {
                throw new ArgumentException("Invalid project ID format");
            }
            ProjectId = parsedProjectId;

            var currentProject = await _projectService.GetProjectById(ProjectId);
            AssocTeamId = currentProject.AssociatedTeam;
            ProjectName = currentProject.ProjectName;
        }
    }
}