using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using CS_Project_Manager.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.Security.Claims;

namespace CS_Project_Manager.Pages
{
    public class TeamLandingModel(TeamService teamService, StudentUserService studentUserService, ProjectService projectService, RequirementService requirementService) : PageModel
    {
        private readonly TeamService _teamService = teamService;
        private readonly StudentUserService _studentUserService = studentUserService;
        private readonly ProjectService _projectService = projectService;
        private readonly RequirementService _requirementService = requirementService;

        [BindProperty]
        public ObjectId TeamId { get; set; }

        [BindProperty]
        public string TeamName { get; set; }

        [BindProperty]
        public List<Project> Projects { get; set; }
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

            Projects = await _projectService.GetProjectsByTeamIdAsync(team.Id);
            foreach (var project in Projects)
            {
                // Calculate project progress
                await ProjectDisplayHelper.CalculateProjectProgressAsync(project.Id, ProjectProgress, _requirementService);
            }
        }
    }
}
