/*
* Prologue
Created By: Anakha Krishna
Date Created: 2/16/25
Last Revised By: Dylan Sailors
Date Revised: 4/13/25
Purpose: The main dashboard page for the website
Preconditions: MongoDBService, ProjectService, TeamService instances properly initialized and injected; Project and Team models must be correctly defined
Postconditions: shows a user a list of their projects and allows them to create a new one
Error and exceptions: ArgumentNullException: username is null or empty; MongoException: issue with the MongoDB connection or operation; InvalidOperationException: data cannot be retrieved
Side effects: N/A
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using MongoDB.Bson;
using CS_Project_Manager.Utilities;
namespace CS_Project_Manager.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ProjectService _projectService;
        private readonly TeamService _teamService;
        private readonly StudentUserService _userService;
        private readonly RequirementService _requirementService;
        // bound property for list of teams and projects
        [BindProperty]
        public List<Team> Teams { get; set; }
        [BindProperty]
        public List<Project> Projects { get; set; }
        // Dictionary to store project progress information
        public Dictionary<ObjectId, decimal> ProjectProgress { get; set; } = new Dictionary<ObjectId, decimal>();
        private readonly ILogger<DashboardModel> _logger;
        public DashboardModel(
            ILogger<DashboardModel> logger,
            ProjectService projectService,
            TeamService teamService,
            StudentUserService userService,
            RequirementService requirementService)
        {
            _logger = logger;
            _teamService = teamService;
            _userService = userService;
            _projectService = projectService;
            _requirementService = requirementService;
            Teams = new List<Team>();
            Projects = new List<Project>();
        }
        public async Task OnGetAsync()
        {
            // check for user login
            if (User.Identity?.IsAuthenticated == true)
            {
                // gets a list of all projects related to the user
                var userObj = await _userService.GetUserByEmailAsync(User.FindFirstValue(ClaimTypes.Email));
                Teams = await _teamService.GetTeamsByStudentIdAsync(userObj.Id);
                foreach (var team in Teams)
                {
                    var projects = await _projectService.GetProjectsByTeamIdAsync(team.Id);
                    foreach (var project in projects)
                    {
                        Projects.Add(project);
                        // Calculate project progress
                        await ProjectDisplayHelper.CalculateProjectProgressAsync(project.Id, ProjectProgress, _requirementService);
                    }
                }
            }
        }
    }
}