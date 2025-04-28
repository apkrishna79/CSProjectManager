/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Anakha Krishna
Date Revised: 4/27/25
Purpose: Handles the logic for displaying the account details of a student user and their enrolled classes and teams

Preconditions: user must be logged in, services (StudentUserService, ClassService, TeamService) must be available
Postconditions: account details with enrolled classes and teams displayed on the account page
Error and exceptions: ArgumentNullException: username is null or empty; MongoException: issue with the MongoDB connection or operation; InvalidOperationException: user data cannot be retrieved
Side effects: N/A
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.Security.Claims;

namespace CS_Project_Manager.Pages
{
    public class AccountModel : PageModel
    {
        private readonly ILogger<AccountModel> _logger;
        private readonly StudentUserService _studentUserService;
        private readonly ClassService _classService;
        private readonly TeamService _teamService;

        public AccountModel(StudentUserService studentUserService, ClassService classService, TeamService teamService, ILogger<AccountModel> logger)
        {
            _studentUserService = studentUserService;
            _classService = classService;
            _teamService = teamService;
            _logger = logger;
        }

        public StudentUser? StudentUser { get; set; }
        public List<Class> EnrolledClasses { get; set; } = new();
        public List<Team> Teams { get; set; } = new();
        public Dictionary<ObjectId, string> ClassIdToName { get; set; } = new();

        // Handle GET request and fetch the account details
        public async Task OnGetAsync()
        {
            // Get email of logged in user
            string email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                _logger.LogInformation("User email retrieved: {Email}", email);
                StudentUser = await _studentUserService.GetUserByEmailAsync(email);
                _logger.LogInformation("Student user found: {UserId}", StudentUser.Id);

                if (StudentUser != null)
                {
                    // Fetch enrolled classes and teams for user
                    EnrolledClasses = await _classService.GetClassesForStudentAsync(StudentUser.Id);
                    _logger.LogInformation("{ClassCount} classes retrieved for user {UserId}", EnrolledClasses.Count, StudentUser.Id);

                    Teams = await _teamService.GetTeamsByStudentIdAsync(StudentUser.Id);
                    _logger.LogInformation("{TeamCount} teams retrieved for user {UserId}", Teams.Count, StudentUser.Id);


                    // Fetch class names for associated classes of the teams
                    var classIds = Teams.Select(t => t.AssociatedClass).Distinct().ToArray();
                    ClassIdToName = await _classService.GetClassIdToNameById(classIds);
                    _logger.LogInformation("Class names retrieved for {ClassCount} classes", classIds.Length);
                }
            }
        }
    }
}