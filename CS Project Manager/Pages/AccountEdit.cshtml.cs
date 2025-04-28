/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/1/25
Last Revised By: Anakha Krishna
Date Revised: 4/27/25
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
using System.Text.RegularExpressions;

namespace CS_Project_Manager.Pages
{
    public class AccountEditModel(StudentUserService studentUserService, ClassService classService, TeamService teamService, ProjectService projectService, RequirementService requirementService, DiscussionBoardService discussionBoardService, ILogger<AccountEditModel> logger) : PageModel
    {
        private readonly ILogger<AccountEditModel> _logger = logger;
        private readonly StudentUserService _studentUserService = studentUserService;
        private readonly ClassService _classService = classService;
        private readonly TeamService _teamService = teamService;
        private readonly ProjectService _projectService = projectService;
        private readonly RequirementService _requirementService = requirementService;
        private readonly DiscussionBoardService _discussionBoardService = discussionBoardService;

        // Properties for user data and selections
        [BindProperty]
        [Required]
        [EmailAddress]
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
        [BindProperty]
        public ObjectId AssociatedClassId { get; set; }

        public StudentUser StudentUser { get; set; }
        public Dictionary<ObjectId, string> ClassIdToName { get; set; } = new();
        [BindProperty]
        public string NewClassName { get; set; }
        [BindProperty]
        public string NewTeamName { get; set; }
        public string ErrorMessage { get; set; }

        // Handle GET request and load user data
        public async Task OnGetAsync()
        {
            await LoadUserDataAsync();
        }

        // Handle email change
        public async Task<IActionResult> OnPostEmailChangedAsync()
        {
            string currentEmail = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(currentEmail))
            {
                _logger.LogInformation("Current user email retrieved: {CurrentEmail}", currentEmail);
                StudentUser = await _studentUserService.GetUserByEmailAsync(currentEmail);

                if (StudentUser != null)
                {
                    _logger.LogInformation("Student user found: {UserId}", StudentUser.Id);
                    var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@ku\.edu$");
                    if (!emailRegex.IsMatch(Email))
                    {
                        _logger.LogWarning("Invalid KU email address provided: {Email}", Email);
                        ModelState.AddModelError(string.Empty, "Email must be a valid KU address.");
                        ModelState.Remove("NewClassName");
                        ModelState.Remove("NewTeamName");
                        await LoadUserDataAsync(); // reload existing state so enrolled classes and teams still show on refresh
                        return Page();
                    }
                    // check for existing email
                    var existingUser = await _studentUserService.GetUserByEmailAsync(Email);
                    if (existingUser != null)
                    {
                        _logger.LogWarning("Email already in use: {Email}", Email);
                        ModelState.AddModelError(string.Empty, "Email is already in use.");
                        ModelState.Remove("NewClassName");
                        ModelState.Remove("NewTeamName");
                        await LoadUserDataAsync(); // reload existing state so enrolled classes and teams still show on refresh
                        return Page(); // Display error if email is already in use
                    }

                    // Step 1: Update the email in DB
                    StudentUser.Email = Email;
                    _logger.LogInformation("Updating email for user {UserId} to {Email}", StudentUser.Id, Email);
                    await _studentUserService.UpdateUserEmailAsync(StudentUser.Id, Email);

                    // Step 2: Rebuild the claims
                    _logger.LogInformation("Rebuilding claims for user {UserId}", StudentUser.Id);
                    var claims = ClaimsHelper.GenerateClaims(StudentUser.FirstName, Email);

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // Step 3: Refresh the auth cookie with the new claims
                    _logger.LogInformation("Refreshing auth cookie for user {UserId}", StudentUser.Id);
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                }
            }

            return RedirectToPage();
        }


        // Adding a class
        public async Task<IActionResult> OnPostAddClassAsync()
        {
            _logger.LogInformation("OnPostAddClassAsync started.");
            await LoadUserDataAsync();
            _logger.LogInformation("User data loaded.");

            if (!string.IsNullOrEmpty(NewClassName)) // If something is typed into the NewClassName field
            {
                _logger.LogInformation("New class name provided: {NewClassName}", NewClassName);
                var existingClass = await _classService.GetClassByNameAsync(NewClassName);
                if (existingClass != null) // If the class entered already exists
                {
                    ErrorMessage = "Class already exists";
                    ModelState.Remove("Email");
                    ModelState.Remove("SelectedClassId");
                    return Page();
                }

                // Else create new class
                var newClass = new Class
                {
                    Name = NewClassName,
                    EnrolledStudents = new List<ObjectId>{ }
                };

                await _classService.CreateClassAsync(newClass);
                _logger.LogInformation("New class created: {ClassId}", newClass.Id);

                SelectedClassId = newClass.Id;

                await _discussionBoardService.CreateDiscussionBoardAsync(new DiscussionBoard // Create new discussion board for that class
                {
                    IsClassBoard = true,
                    ClassId = SelectedClassId,
                    TeamId = ObjectId.Empty
                });
                _logger.LogInformation("Discussion board created for class: {ClassId}", SelectedClassId);
            }

            if (StudentUser != null && SelectedClassId != ObjectId.Empty) // Add student to the class
            {
                _logger.LogInformation("Adding student {StudentId} to class {ClassId}", StudentUser.Id, SelectedClassId);
                await _classService.AddStudentToClassAsync(SelectedClassId, StudentUser.Id);
            }

            _logger.LogInformation("OnPostAddClassAsync completed.");
            return RedirectToPage();
        }

        // Removing a class
        public async Task<IActionResult> OnPostRemoveClassAsync(ObjectId classId)
        {
            _logger.LogInformation("OnPostRemoveClassAsync started.");
            await LoadUserDataAsync();

            if (StudentUser != null && classId != ObjectId.Empty)
            {
                logger.LogInformation("Removing student {StudentId} from class {ClassId}", StudentUser.Id, classId);
                await _classService.RemoveStudentFromClassAsync(classId, StudentUser.Id);
            }

            _logger.LogInformation("OnPostRemoveClassAsync completed.");
            return RedirectToPage();
        }

        // Adding a team
        public async Task<IActionResult> OnPostAddTeamAsync()
        {
            _logger.LogInformation("OnPostAddTeamAsync started.");
            await LoadUserDataAsync();
            _logger.LogInformation("User data loaded.");

            if (StudentUser != null && SelectedTeamId != ObjectId.Empty)
            {
                _logger.LogInformation("Selected team ID: {SelectedTeamId}", SelectedTeamId);

                var selectedTeam = await _teamService.GetTeamByIdAsync(SelectedTeamId);
                if (selectedTeam != null)
                {
                    _logger.LogInformation("Selected team found: {TeamId}", selectedTeam.Id);
                    // Check if the student is already enrolled in another team for the same class
                    var existingTeam = Teams.FirstOrDefault(t => t.AssociatedClass == selectedTeam.AssociatedClass);
                    if (existingTeam != null)
                    {
                        _logger.LogWarning("Student {StudentId} is already enrolled in a team for class {ClassId}", StudentUser.Id, selectedTeam.AssociatedClass);
                        ErrorMessage = "You are already enrolled in a team for this class.";
                        ModelState.Remove("Email");
                        return Page();
                    }
                    else
                    {
                        _logger.LogInformation("Adding student {StudentId} to team {TeamId}", StudentUser.Id, SelectedTeamId);
                        await _teamService.AddStudentToTeamAsync(SelectedTeamId, StudentUser.Id);
                    }
                }
            }

            _logger.LogInformation("OnPostAddTeamAsync completed.");
            return RedirectToPage();
        }

        // Create a new team
        public async Task<IActionResult> OnPostCreateTeamAsync()
        {
            _logger.LogInformation("OnPostCreateTeamAsync started.");
            await LoadUserDataAsync();
            _logger.LogInformation("User data loaded.");

            // Check to make sure a new team that doesn't already exist with same name for that same class
            var teams = await _teamService.GetAllTeams();
            _logger.LogInformation("{TeamCount} teams retrieved.", teams.Count);

            var existingTeam = teams.FirstOrDefault(t => t.Name == NewTeamName && t.AssociatedClass == AssociatedClassId);

            if (existingTeam != null)
            {
                _logger.LogWarning("Team already exists for class {ClassId}: {TeamName}", AssociatedClassId, NewTeamName);
                ErrorMessage = "Team already exists for this class";
                ModelState.Remove("Email");
                ModelState.Remove("NewClassName");
                return Page();
            }

            // Check if user is already in a team for the class
            var alreadyInTeam = Teams.FirstOrDefault(t => t.AssociatedClass == AssociatedClassId);
            if (alreadyInTeam != null)
            {
                _logger.LogWarning("Student {StudentId} is already enrolled in a team for class {ClassId}", StudentUser.Id, AssociatedClassId);
                ErrorMessage = "You are already enrolled in a team for this class.";
                ModelState.Remove("Email");
                ModelState.Remove("NewClassName");
                return Page();
            }

            // Else make new team
            var newTeam = new Team
            {
                Name = NewTeamName,
                AssociatedClass = AssociatedClassId,
                Members = new List<ObjectId> { StudentUser.Id }
            };

            await _teamService.CreateTeamAsync(newTeam);
            _logger.LogInformation("New team created: {TeamId}", newTeam.Id);
            var teamId = newTeam.Id;

            await _discussionBoardService.CreateDiscussionBoardAsync(new DiscussionBoard // Make team discussion board
            {
                IsClassBoard = false,
                ClassId = ObjectId.Empty,
                TeamId = teamId
            });
            _logger.LogInformation("Discussion board created for team: {TeamId}", teamId);

            _logger.LogInformation("OnPostCreateTeamAsync completed.");
            return RedirectToPage();
        }

        // Removing a team
        public async Task<IActionResult> OnPostRemoveTeamAsync(ObjectId teamId)
        {
            _logger.LogInformation("OnPostRemoveTeamAsync started.");
            await LoadUserDataAsync();
            _logger.LogInformation("User data loaded.");

            if (StudentUser != null && teamId != ObjectId.Empty)
            {
                _logger.LogInformation("Removing student {StudentId} from team {TeamId}", StudentUser.Id, teamId);
                await _teamService.RemoveStudentFromTeamAsync(teamId, StudentUser.Id);
            }

            _logger.LogInformation("Removing user from assigned requirements for team {TeamId}", teamId);
            await RemoveUserFromAssignedRequirements(teamId);

            _logger.LogInformation("OnPostRemoveTeamAsync completed.");
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
