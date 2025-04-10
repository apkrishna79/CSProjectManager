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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CS_Project_Manager.Utilities;

namespace CS_Project_Manager.Pages
{
    public class AccountEditModel(StudentUserService studentUserService, ClassService classService, TeamService teamService, ProjectService projectService, RequirementService requirementService) : PageModel
    {
        private readonly StudentUserService _studentUserService = studentUserService;
        private readonly ClassService _classService = classService;
        private readonly TeamService _teamService = teamService;
        private readonly ProjectService _projectService = projectService;
        private readonly RequirementService _requirementService = requirementService;

        // Properties for user data and selections
        [BindProperty]
        [Required]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@ku\.edu$", ErrorMessage = "Email must be a valid KU address.")]
        [MaxLength(255)]
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
            if (!ModelState.IsValid)
            {
                await LoadUserDataAsync(); // reload existing state so enrolled classes and teams still show on refresh
                return Page();
            }

            string currentEmail = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(currentEmail))
            {
                StudentUser = await _studentUserService.GetUserByEmailAsync(currentEmail);

                if (StudentUser != null)
                {
                    // Step 0: check for existing email
                    var existingUser = await _studentUserService.GetUserByEmailAsync(Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError(string.Empty, "Email is already in use.");
                        await LoadUserDataAsync(); // reload existing state so enrolled classes and teams still show on refresh
                        return Page(); // Display error if username is already in use
                    }

                    // Step 1: Update the email in DB
                    StudentUser.Email = Email;
                    await _studentUserService.UpdateUserEmailAsync(StudentUser.Id, Email);

                    // Step 2: Rebuild the claims
                    var claims = ClaimsHelper.GenerateClaims(StudentUser.FirstName, Email);

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // Step 3: Refresh the auth cookie with the new claims
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
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

            await RemoveUserFromAssignedRequirements(teamId);

            return RedirectToPage();
        }

        // Used to remove users from assigned requirements when they leave a team, or when leaving a class causes them to leave a team
        private async Task RemoveUserFromAssignedRequirements(ObjectId teamId)
        {
            // Fetch all projects related to the team
            var teamProjects = await _projectService.GetProjectsByTeamIdAsync(teamId);
            var projectIds = teamProjects.Select(p => p.Id).ToList();

            if (projectIds.Any())
            {
                // Fetch all related requirements in one query
                var projectRequirements = await _requirementService.GetRequirementsByProjectIdsAsync(projectIds);

                // Filter requirements where the student is assigned
                var updatedRequirements = projectRequirements
                    .Where(req => req.Assignees.Contains(StudentUser.Id))
                    .Select(req =>
                    {
                        req.Assignees.Remove(StudentUser.Id);
                        return req;
                    })
                    .ToList();

                // Update all modified requirements in a batch operation
                if (updatedRequirements.Any())
                {
                    await _requirementService.UpdateRequirementsAsync(updatedRequirements);
                }
            }
        }

        // Load user data and handle data conflicts
        private async Task LoadUserDataAsync()
        {
            string email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                StudentUser = await _studentUserService.GetUserByEmailAsync(email);

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
                    await RemoveUserFromAssignedRequirements(team.Id);
                }
                Teams = Teams.Except(conflictingTeams).ToList();
            }
        }
    }
}
