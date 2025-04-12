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
    public class AccountEditModel(StudentUserService studentUserService, ClassService classService, TeamService teamService, ProjectService projectService, RequirementService requirementService, DiscussionBoardService discussionBoardService) : PageModel
    {
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
                StudentUser = await _studentUserService.GetUserByEmailAsync(currentEmail);

                if (StudentUser != null)
                {
                    // Step 0: check for existing email
                    var existingUser = await _studentUserService.GetUserByEmailAsync(Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError(string.Empty, "Email is already in use.");
                        ModelState.Remove("NewClassName");
                        ModelState.Remove("NewTeamName");
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

            if (!string.IsNullOrEmpty(NewClassName)) // If something is typed into the NewClassName field
            {
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

                SelectedClassId = newClass.Id;

                await _discussionBoardService.CreateDiscussionBoardAsync(new DiscussionBoard // Create new discussion board for that class
                {
                    IsClassBoard = true,
                    ClassId = SelectedClassId,
                    TeamId = ObjectId.Empty
                });
            }

            if (StudentUser != null && SelectedClassId != ObjectId.Empty) // Add student to the class
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

        // Create a new team
        public async Task<IActionResult> OnPostCreateTeamAsync()
        {
            await LoadUserDataAsync();

            // Check to make sure a new team that doesn't already exist with same name for that same class
            var existingTeam = Teams.FirstOrDefault(t => t.Name == NewTeamName && t.AssociatedClass == AssociatedClassId);

            if (existingTeam != null)
            {
                ErrorMessage = "Team already exists for this class";
                ModelState.Remove("Email");
                ModelState.Remove("NewClassName");
                return Page();
            }

            // Check if user is already in a team for the class
            var alreadyInTeam = Teams.FirstOrDefault(t => t.AssociatedClass == AssociatedClassId);
            if (alreadyInTeam != null)
            {
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
            var teamId = newTeam.Id;

            await _discussionBoardService.CreateDiscussionBoardAsync(new DiscussionBoard // Make team discussion board
            {
                IsClassBoard = false,
                ClassId = ObjectId.Empty,
                TeamId = teamId
            });

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
