/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Anakha Krishna
Date Revised: 3/2/25
Purpose: Handles the logic for editing account details: email, enrolled classes, and teams
Preconditions: User must be logged in, services (StudentUserService, ClassService, TeamService) must be available
Postconditions: Account details are updated, conflicting teams are removed
Error and exceptions: ArgumentNullException, MongoException, InvalidOperationException
Side effects: N/A
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.Security.Claims;

namespace CS_Project_Manager.Pages
{
    public class AccountEditModel : PageModel
    {
        private readonly StudentUserService _studentUserService;
        private readonly ClassService _classService;
        private readonly TeamService _teamService;

        // Initialize services
        public AccountEditModel(StudentUserService studentUserService, ClassService classService, TeamService teamService)
        {
            _studentUserService = studentUserService;
            _classService = classService;
            _teamService = teamService;
        }

        // Properties for user data and selections
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public List<Class> EnrolledClasses { get; set; } = new();

        [BindProperty]
        public List<Team> Teams { get; set; } = new();

        public List<Class> AvailableClasses { get; set; } = new();
        public List<Team> AvailableTeams { get; set; } = new();

        [BindProperty]
        public ObjectId SelectedClassId { get; set; }

        [BindProperty]
        public ObjectId SelectedTeamId { get; set; }

        public StudentUser StudentUser { get; set; }
        public Dictionary<ObjectId, string> ClassIdToName { get; set; } = new();
        public string ErrorMessage { get; set; }

        // Handle GET request and load user data
        public async Task OnGetAsync()
        {
            await LoadUserDataAsync();
        }

        // Handle email change
        public async Task<IActionResult> OnPostEmailChangedAsync()
        {
            string username = User.FindFirstValue(ClaimTypes.Name);
            if (!string.IsNullOrEmpty(username))
            {
                StudentUser = await _studentUserService.GetUserByUsernameAsync(username);

                if (StudentUser != null)
                {
                    // Update the user's email if it changed
                    StudentUser.Email = Email;
                    await _studentUserService.UpdateUserEmailAsync(StudentUser.Id, Email);
                }
            }

            return RedirectToPage();
        }

        // Adding a class
        public async Task<IActionResult> OnPostAddClassAsync()
        {
            await LoadUserDataAsync();

            if (StudentUser != null && SelectedClassId != ObjectId.Empty)
            {
                await _classService.AddStudentToClassAsync(SelectedClassId, StudentUser.Id);
            }

            return RedirectToPage();
        }

        // Removing a class
        public async Task<IActionResult> OnPostRemoveClassAsync(ObjectId classId)
        {
            await LoadUserDataAsync();

            if (StudentUser != null && classId != ObjectId.Empty)
            {
                await _classService.RemoveStudentFromClassAsync(classId, StudentUser.Id);
            }

            return RedirectToPage();
        }

        // Adding a team
        public async Task<IActionResult> OnPostAddTeamAsync()
        {
            await LoadUserDataAsync();

            if (StudentUser != null && SelectedTeamId != ObjectId.Empty)
            {
                var selectedTeam = await _teamService.GetTeamByIdAsync(SelectedTeamId);
                if (selectedTeam != null)
                {
                    // Check if the student is already enrolled in another team for the same class
                    var existingTeam = Teams.FirstOrDefault(t => t.AssociatedClass == selectedTeam.AssociatedClass);
                    if (existingTeam != null)
                    {
                        ErrorMessage = "You are already enrolled in a team for this class.";
                        ModelState.Remove("Email");
                        return Page();
                    }
                    else
                    {
                        await _teamService.AddStudentToTeamAsync(SelectedTeamId, StudentUser.Id);
                    }
                }
            }

            return RedirectToPage();
        }

        // Removing a team
        public async Task<IActionResult> OnPostRemoveTeamAsync(ObjectId teamId)
        {
            await LoadUserDataAsync();

            if (StudentUser != null && teamId != ObjectId.Empty)
            {
                await _teamService.RemoveStudentFromTeamAsync(teamId, StudentUser.Id);
            }

            return RedirectToPage();
        }

        // Load user data and handle data conflicts
        private async Task LoadUserDataAsync()
        {
            string username = User.FindFirstValue(ClaimTypes.Name);
            if (!string.IsNullOrEmpty(username))
            {
                StudentUser = await _studentUserService.GetUserByUsernameAsync(username);

                if (StudentUser != null)
                {
                    Email = StudentUser.Email;
                    EnrolledClasses = await _classService.GetClassesForStudentAsync(StudentUser.Id);
                    Teams = await _teamService.GetTeamsByStudentIdAsync(StudentUser.Id);
                }
            }

            var allClasses = await _classService.GetAllClasses();
            var allTeams = await _teamService.GetAllTeams();

            // Fetch class names for associated classes of all available teams
            var classIds = allTeams.Select(t => t.AssociatedClass).Distinct().ToArray();
            ClassIdToName = await _classService.GetClassIdToNameById(classIds);

            // Update team names to include class names
            foreach (var team in allTeams)
            {
                if (ClassIdToName.ContainsKey(team.AssociatedClass))
                {
                    team.Name += $" ({ClassIdToName[team.AssociatedClass]})";
                }
            }

            // Filter out classes and teams the student is already a part of
            var enrolledClassIds = EnrolledClasses.Select(c => c.Id).ToHashSet();
            var enrolledTeamIds = Teams.Select(t => t.Id).ToHashSet();

            AvailableClasses = allClasses.Where(c => !enrolledClassIds.Contains(c.Id)).ToList();
            AvailableTeams = allTeams.Where(t => !enrolledTeamIds.Contains(t.Id) && enrolledClassIds.Contains(t.AssociatedClass)).ToList();

            // Check for conflicting teams
            var conflictingTeams = Teams.Where(t => !enrolledClassIds.Contains(t.AssociatedClass)).ToList();
            if (conflictingTeams.Any())
            {
                ErrorMessage = "You are part of teams associated with classes you are not enrolled in. You have been removed from these teams.";
                foreach (var team in conflictingTeams)
                {
                    await _teamService.RemoveStudentFromTeamAsync(team.Id, StudentUser.Id);
                }
                Teams = Teams.Except(conflictingTeams).ToList();
            }
        }
    }
}