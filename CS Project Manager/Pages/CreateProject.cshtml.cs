/*
 * Prologue
Created By: Jackson Wunderlich
Date Created: 2/19/25
Last Revised By: Jackson Wunderlich
Date Revised: 3/1/25
Purpose: Handles project creation
Preconditions: MongoDBService, ProjectService, TeamService, ClassService instances properly initialized and injected; Project, Class, Team models must be correctly defined
Postconditions: new project is created and stored in the database if the inputs are valid
Error and exceptions: ArgumentNullException (required field empty)
Side effects: N/A
Invariants: _projectService, _teamService, _classService fields are always initialized with valid instance, OnPostAsync method always returns an IActionResult
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using System.Security.Claims;

namespace CS_Project_Manager.Pages
{
    public class ProjectModel : PageModel
    {
        // initializes injected service for operations using projects, teams, and classes
        private readonly ProjectService _projectService;
        private readonly TeamService _teamService;
        private readonly ClassService _classService;
        private readonly StudentUserService _userService;

        // bound property for list of teams
        [BindProperty]
        public List<Team> Teams { get; set; }

        // creates bound properties for project creation form input
        [BindProperty]
        [Required]
        [MaxLength(100)]
        public required string ProjectName { get; set; }

        [BindProperty]
        [Required]
        public required string Description { get; set; }

        [BindProperty]
        [Required]
        public required ObjectId SelectedTeamId { get; set; }

        // dictionary to form team-class pairs for user input
        public Dictionary<Team, string> TeamAndClass { get; set; } = [];

        public ProjectModel(ProjectService projectService, TeamService teamService, ClassService classService, StudentUserService userService)
        {
            // initialize services
            _projectService = projectService;
            _teamService = teamService;
            _classService = classService;
            _userService = userService;
            // create list and dictionary needed for operations
            Teams = new List<Team>();
            TeamAndClass = new Dictionary<Team, string>();
        }
        
        public async Task OnGetAsync()
        {
            var userObj = await _userService.GetUserByEmailAsync(User.FindFirstValue(ClaimTypes.Email));
            Teams = await _teamService.GetTeamsByStudentIdAsync(userObj.Id);
            // create a dictionary with key: Team object and value: corresponding class name
            foreach (var team in Teams)
            {
                var class_name = await _classService.GetClassByIdAsync(team.AssociatedClass);
                TeamAndClass.Add(team, class_name.Name);
            }
        }

        // runs when the create project button is pressed
        public async Task<IActionResult> OnPostAsync()
        {
            var newProject = new Project
            {
                ProjectName = ProjectName,
                Description = Description,
                AssociatedTeam = SelectedTeamId
            };
            await _projectService.CreateProjectAsync(newProject);
            return RedirectToPage("/RequirementsStack", new { projectId = newProject.Id.ToString() });
        }


    }
}
