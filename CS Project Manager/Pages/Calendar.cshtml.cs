/*
* Prologue
Created By: Jackson Wunderlich
Date Created: 3/24/25
Last Revised By: Jackson Wunderlich
Date Revised: 3/26/25
Purpose: page displaying a calendar that allows users to set and view availability and create meetings
Preconditions: MongoDBService, other service instances properly initialized and injected; CalendarItem must be correctly defined
Error and exceptions: ArgumentNullException: username is null or empty; MongoException: issue with the MongoDB connection or operation; InvalidOperationException: data cannot be retrieved
Side effects: N/A
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class CalendarModel : PageModel
    {
        private readonly TeamService _teamService;
        private readonly StudentUserService _userService;
        private readonly CalendarService _calendarService;

        // bound property for the team related to the project
        [BindProperty]
        public Team ProjectTeam { get; set; }
        // bound property for the list of calendar events related to the team
        [BindProperty]
        public List<CalendarItem> TeamCalendarItems { get; set; }

        public List<String> Days { get; set; }
        public List<String> Times { get; set; }

        private readonly ILogger<DashboardModel> _logger;

        public CalendarModel(ILogger<DashboardModel> logger, TeamService teamService, StudentUserService userService, CalendarService calendarService)
        {
            _logger = logger;
            _teamService = teamService;
            _userService = userService;
            _calendarService = calendarService;
            // creates lists of days and times for the calendar
            Days = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
            Times = ["9:00", "9:30", "10:00", "10:30", "11:00", "11:30", "12:00", "12:30", "1:00", "1:30", "2:00", "2:30", "3:00", "3:30", "4:00", "4:30", "5:00", "5:30"];
        }

        public async Task OnGetAsync(ObjectId teamId)
        {
            // gets the team related to the current project and a list of all calendar items for that team
            ProjectTeam = await _teamService.GetTeamByIdAsync(teamId);
            TeamCalendarItems = await _calendarService.GetCalendarItemsByTeamIdAsync(ProjectTeam.Id);
        }
    }
}