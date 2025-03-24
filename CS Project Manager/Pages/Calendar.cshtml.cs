/*
* Prologue
Created By: Jackson Wunderlich
Date Created: 3/24/25
Last Revised By: Jackson Wunderlich
Date Revised: 3/24/25
Purpose: 
Preconditions: 
Error and exceptions: ArgumentNullException: username is null or empty; MongoException: issue with the MongoDB connection or operation; InvalidOperationException: data cannot be retrieved
Side effects: N/A
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CS_Project_Manager.Pages
{
    public class CalendarModel : PageModel
    {
        private readonly TeamService _teamService;
        private readonly StudentUserService _userService;

        // bound property for list of teams
        [BindProperty]
        public List<Team> Teams { get; set; }

        private readonly ILogger<DashboardModel> _logger;

        public CalendarModel(ILogger<DashboardModel> logger, TeamService teamService, StudentUserService userService)
        {
            _logger = logger;
            _teamService = teamService;
            _userService = userService;
            Teams = new List<Team>();
        }

        public async Task OnGetAsync()
        {
            // gets a list of all teams related to the user
            var username = User.FindFirstValue(ClaimTypes.Name);
            var userObj = await _userService.GetUserByUsernameAsync(username);
            Teams = await _teamService.GetTeamsByStudentIdAsync(userObj.Id);
        }
    }
}