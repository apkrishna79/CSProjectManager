using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.Security.Claims;

namespace CS_Project_Manager.Pages
{
    public class TeamLandingModel(TeamService teamService, StudentUserService studentUserService) : PageModel
    {
        private readonly TeamService _teamService = teamService;
        private readonly StudentUserService _studentUserService = studentUserService;

        [BindProperty]
        public ObjectId TeamId { get; set; }

        public async Task OnGetAsync(string teamId)
        {
            if (!ObjectId.TryParse(teamId, out ObjectId parsedTeamId))
            {
                throw new ArgumentException("Invalid Team ID format");
            }
            TeamId = parsedTeamId;

            string currentEmail = User.FindFirstValue(ClaimTypes.Email);
            StudentUser currentUser = await _studentUserService.GetUserByEmailAsync(currentEmail);
            if (!(await _teamService.GetTeamByIdAsync(TeamId)).Members.Contains(currentUser.Id))
            {
                throw new UnauthorizedAccessException("You do not have access to this team.");
            }
        }
    }
}
