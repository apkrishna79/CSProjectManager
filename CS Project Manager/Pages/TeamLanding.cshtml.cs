/*
 * Prologue
Created By: Isabel Loney
Date Created: 4/17/25
Last Revised By: Isabel Loney
Date Revised: 4/26/25
Purpose: Team landing page to display team members, team projects, and events
Preconditions: Services properly initialized and injected; models correctly defined
Postconditions: Can create new projects
Error and exceptions: ArgumentNullException (required field empty)
Side effects: N/A
Invariants: private service fields are always initialized with valid instance
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using CS_Project_Manager.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.Security.Claims;

namespace CS_Project_Manager.Pages
{
    public class TeamLandingModel(TeamService teamService, StudentUserService studentUserService, ProjectService projectService, RequirementService requirementService, CalendarService calendarService) : PageModel
    {
        private readonly TeamService _teamService = teamService;
        private readonly StudentUserService _studentUserService = studentUserService;
        private readonly ProjectService _projectService = projectService;
        private readonly RequirementService _requirementService = requirementService;
        private readonly CalendarService _calendarService = calendarService;

        [BindProperty]
        public ObjectId TeamId { get; set; }

        [BindProperty]
        public string TeamName { get; set; }

        [BindProperty]
        public List<Project> Projects { get; set; } = [];

        [BindProperty]
        public List<CalendarItem> TeamCalendarItems { get; set; }
        [BindProperty]
        public List<string> Members { get; set; } = [];

        public Dictionary<ObjectId, decimal> ProjectProgress { get; set; } = new Dictionary<ObjectId, decimal>();

        public async Task OnGetAsync(string teamId)
        {
            if (!ObjectId.TryParse(teamId, out ObjectId parsedTeamId))
            {
                throw new ArgumentException("Invalid Team ID format");
            }
            TeamId = parsedTeamId;
            Team team = await _teamService.GetTeamByIdAsync(TeamId);

            string currentEmail = User.FindFirstValue(ClaimTypes.Email);
            StudentUser currentUser = await _studentUserService.GetUserByEmailAsync(currentEmail);
            if (!team.Members.Contains(currentUser.Id))
            {
                throw new UnauthorizedAccessException("You do not have access to this team.");
            }

            TeamName = team.Name;

            Projects = await _projectService.GetProjectsByTeamIdAsync(TeamId);
            foreach (var project in Projects)
            {
                // Calculate project progress
                await ProjectDisplayHelper.CalculateProjectProgressAsync(project.Id, ProjectProgress, _requirementService);
            }

            TeamCalendarItems = await _calendarService.GetCalendarItemsByTeamIdAsync(TeamId);

            foreach (var memberId in team.Members)
            {
                var user = await _studentUserService.GetUserByIdAsync(memberId);
                Members.Add($"{user.FirstName} {user.LastName}");
            }
        }

        // converts UTC time to Central Standard Time
        public DateTime ConvertToCentralTime(DateTime utcDateTime)
        {
            TimeZoneInfo centralZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, centralZone);
        }
    }
}
