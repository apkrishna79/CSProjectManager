/*
* Prologue
Created By: Jackson Wunderlich
Date Created: 3/24/25
Last Revised By: Dylan Sailors
Date Revised: 3/30/25
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
        private readonly UserAvailabilityService _userAvailabilityService;

        // bound property for the team related to the project
        [BindProperty]
        public Team ProjectTeam { get; set; }

        // bound property for the list of calendar events related to the team
        [BindProperty]
        public List<CalendarItem> TeamCalendarItems { get; set; }

        // bound property for the list of user availability items related to the team
        [BindProperty]
        public List<UserAvailability> UserAvailabilityItems { get; set; }

        [BindProperty]
        public CalendarItem NewCalendarItem { get; set; } = new CalendarItem
        {
            EventName = string.Empty
        };
        
        [BindProperty]
        public List<string> SelectedDays { get; set; } = new List<string>();

        [BindProperty]
        public List<string> SelectedTimes { get; set; } = new List<string>();
        
        [BindProperty]
        public List<string> RemoveIds { get; set; } = new List<string>();
        
        public List<String> Days { get; set; }
        public List<String> Times { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public ObjectId TeamId { get; set; }

        [BindProperty]
        public ObjectId CurrentUserId { get; set; }
        
        [BindProperty]
        public List<DisplayUserViewModel> TeamMemberDisplayOptions { get; set; } = new List<DisplayUserViewModel>();

        private readonly ILogger<DashboardModel> _logger;

        public CalendarModel(ILogger<DashboardModel> logger, TeamService teamService, StudentUserService userService, CalendarService calendarService, UserAvailabilityService userAvailabilityService)
        {
            _logger = logger;
            _teamService = teamService;
            _userService = userService;
            _calendarService = calendarService;
            _userAvailabilityService = userAvailabilityService;
            UserAvailabilityItems = new List<UserAvailability>();
            // creates lists of days and times for the calendar
            Days = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
            Times = ["8:00 AM", "8:30 AM", "9:00 AM", "9:30 AM", "10:00 AM", "10:30 AM", "11:00 AM", "11:30 AM", "12:00 PM", "12:30 PM", "1:00 PM", "1:30 PM", "2:00 PM",
                     "2:30 PM", "3:00 PM", "3:30 PM", "4:00 PM", "4:30 PM", "5:00 PM", "5:30 PM", "6:00 PM", "6:30 PM", "7:00 PM", "7:30 PM", "8:00 PM", "8:30 PM"];
        }

        public async Task OnGetAsync(ObjectId teamId)
        {
            ProjectTeam = await _teamService.GetTeamByIdAsync(teamId);
            var currentUser = await _userService.GetUserByUsernameAsync(User.Identity.Name);
            if (currentUser != null)
            {
                CurrentUserId = currentUser.Id;
            }
            TeamCalendarItems = await _calendarService.GetCalendarItemsByTeamIdAsync(ProjectTeam.Id);
            await LoadUserAvailabilityAsync(teamId);
        }

        // add or update calendar item
        public async Task<IActionResult> OnPostAddOrUpdateCalendarItemAsync(ObjectId teamId)
        {
            TeamId = teamId;
            var existingCalendarItem = await _calendarService.GetCalendarItemByIdAsync(NewCalendarItem.Id);

            if (existingCalendarItem != null)
            {
                // Update existing item
                existingCalendarItem.EventName = NewCalendarItem.EventName;
                existingCalendarItem.StartDateTime = NewCalendarItem.StartDateTime;
                existingCalendarItem.EndDateTime = NewCalendarItem.EndDateTime;
                existingCalendarItem.Notes = NewCalendarItem.Notes;
                await _calendarService.UpdateCalendarItemAsync(existingCalendarItem);
            }
            else
            {
                // Add new item
                NewCalendarItem.AssocTeamId = TeamId;
                await _calendarService.AddCalendarItemAsync(NewCalendarItem);
            }
            TeamCalendarItems = await _calendarService.GetCalendarItemsByTeamIdAsync(TeamId);
            await LoadUserAvailabilityAsync(teamId);
            return RedirectToPage("/Calendar", new { teamId = TeamId });
        }

        // handles adding and updating availabilities for the user
        public async Task<IActionResult> OnPostUpdateAvailabilityAsync(ObjectId teamId)
        {
            TeamId = teamId;
            ProjectTeam = await _teamService.GetTeamByIdAsync(teamId);
            if (ProjectTeam == null)
            {
                ModelState.AddModelError(string.Empty, "Team not found.");
                return Page();
            }
            var currentUser = await _userService.GetUserByUsernameAsync(User.Identity.Name);
            if (currentUser == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }
            for (int i = 0; i < SelectedDays.Count; i++)
            {
                if (i < SelectedTimes.Count)
                {
                    var newAvailability = new UserAvailability
                    {
                        Id = ObjectId.GenerateNewId(),
                        Day = SelectedDays[i],
                        Time = SelectedTimes[i],
                        AssocUserId = currentUser.Id,
                        AssocTeamId = teamId
                    };
                    await _userAvailabilityService.AddUserAvailabilityAsync(newAvailability);
                }
            }
            foreach (var idString in RemoveIds)
            {
                if (ObjectId.TryParse(idString, out ObjectId id))
                {
                    var availability = await _userAvailabilityService.GetUserAvailabilityByIdAsync(id);
                    if (availability != null && availability.AssocUserId == currentUser.Id)
                    {
                        await _userAvailabilityService.DeleteUserAvailabilityAsync(id);
                    }
                }
            }
            await LoadUserAvailabilityAsync(teamId);
            return RedirectToPage("/Calendar", new { teamId });
        }

        // converts UTC time to Central Standard Time
        public DateTime ConvertToCentralTime(DateTime utcDateTime)
        {
            TimeZoneInfo centralZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, centralZone);
        }

        // loads the user's availabilities
        private async Task LoadUserAvailabilityAsync(ObjectId teamId)
        {
            if (ProjectTeam == null)
            {
                ProjectTeam = await _teamService.GetTeamByIdAsync(teamId);
            }
            UserAvailabilityItems = new List<UserAvailability>();
            TeamMemberDisplayOptions = new List<DisplayUserViewModel>();
            var currentUser = await _userService.GetUserByUsernameAsync(User.Identity.Name);
            if (ProjectTeam?.Members != null)
            {
                foreach (var userId in ProjectTeam.Members)
                {
                    var teamMember = await _userService.GetUserByIdAsync(userId);
                    if (teamMember != null && teamMember.Id != currentUser?.Id)
                    {
                        TeamMemberDisplayOptions.Add(new DisplayUserViewModel 
                        { 
                            UserId = teamMember.Id,
                            Username = teamMember.Username,
                            IsDisplayed = true
                        });
                    }
                    var userAvailabilities = await _userAvailabilityService.GetUserAvailabilityByUserIdAsync(userId);
                    foreach (var item in userAvailabilities)
                    {
                        if (item.AssocTeamId != teamId)
                        {
                            item.AssocTeamId = teamId;
                            await _userAvailabilityService.UpdateUserAvailabilityTeamIdAsync(userId, teamId);
                        }
                        UserAvailabilityItems.Add(item);
                    }
                }
            }
        }
        
        // update availability when the user changes teams
        public async Task UpdateUserAvailabilityOnTeamChangeAsync(ObjectId userId, ObjectId newTeamId)
        {
            await _userAvailabilityService.UpdateUserAvailabilityTeamIdAsync(userId, newTeamId);
        }
    }
    
    // aids in displaying the view model and team members' availabilities
    public class DisplayUserViewModel
    {
        public ObjectId UserId { get; set; }
        public string Username { get; set; }
        public bool IsDisplayed { get; set; } = true;
    }
}
