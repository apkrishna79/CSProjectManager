/*
 * Prologue
Created By: Isabel Loney
Date Created: 2/25/25
Last Revised By: Isabel Loney
Date Revised: 4/10/25
Purpose: Handles user registration by creating new accounts, validating inputs, checking for existing accounts

Preconditions: MongoDBService, StudentUserService, TeamService, ClassService instances properly initialized and injected; StudentUser, Team, and Class model must be correctly defined
Postconditions: new user account is created and stored in the MongoDB database if the inputs are valid and the username is not already in use, user is redirected to the Dashboard page upon successful registration
Error and exceptions: ArgumentNullException (required field empty)
Side effects: N/A
Invariants: _mongoDBService, _userService, _teamService, and _classService fields are always initialized with valid instances, OnPostAsync method always returns an IActionResult
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using CS_Project_Manager.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace CS_Project_Manager.Pages
{
    public class RegisterModel(StudentUserService studentUserService, ClassService classService, TeamService teamService, DiscussionBoardService discussionBoardService) : PageModel
    {
        // Injected services for database operations and user-specific operations
        private readonly StudentUserService _studentUserService = studentUserService;
        private readonly ClassService _classService = classService;
        private readonly TeamService _teamService = teamService;
        private readonly DiscussionBoardService _discussionBoardService = discussionBoardService;

        // Bound properties to hold input values from the registration form
        [BindProperty]
        [Required]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@ku\.edu$", ErrorMessage = "Email must be a valid KU address.")]
        [MaxLength(255)]
        public string Email { get; set; }

        [BindProperty]
        [Required]
        public required string Password { get; set; }

        [BindProperty]
        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [BindProperty]
        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        [BindProperty]
        public List<string> EnrolledClasses { get; set; } = [];

        [BindProperty]
        public Dictionary<string, string> SelectedTeams { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // return page with validation errors
            }

            // Check if an account already exists with the given username
            var existingUser = await _studentUserService.GetUserByEmailAsync(Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Email is already in use.");
                return Page(); // Display error if username is already in use
            }

            var newUser = new StudentUser
            {
                Email = Email,
                PasswordHash = PasswordHelper.HashPassword(Password),
                FirstName = FirstName,
                LastName = LastName,
            };

            await _studentUserService.CreateUserAsync(newUser);
            var studentUserId = newUser.Id;

            foreach (var className in EnrolledClasses)
            {
                var existingClass = await _classService.GetClassByNameAsync(className);
                ObjectId classId;

                if (existingClass == null)
                {
                    var newClass = new Class
                    {
                        Name = className,
                        EnrolledStudents = new List<ObjectId> { studentUserId }
                    };
                    await _classService.CreateClassAsync(newClass);
                    classId = newClass.Id;

                    await _discussionBoardService.CreateDiscussionBoardAsync(new DiscussionBoard
                    {
                        IsClassBoard = true,
                        ClassId = classId,
                        TeamId = ObjectId.Empty
                    });
                }
                else
                {
                    classId = existingClass.Id;

                    if (!existingClass.EnrolledStudents.Contains(studentUserId))
                    {
                        await _classService.AddStudentToClassAsync(classId, studentUserId);
                    }
                }

                if (SelectedTeams.TryGetValue(className, out var teamName) && !string.IsNullOrWhiteSpace(teamName))
                {
                    var existingTeam = await _teamService.GetTeamByNameAndClassId(teamName, classId);
                    ObjectId teamId;

                    if (existingTeam == null)
                    {
                        var newTeam = new Team
                        {
                            Name = teamName,
                            AssociatedClass = classId,
                            Members = new List<ObjectId> { studentUserId }
                        };
                        await _teamService.CreateTeamAsync(newTeam);
                        teamId = newTeam.Id;
                    }
                    else
                    {
                        teamId = existingTeam.Id;
                        await _teamService.AddStudentToTeamAsync(teamId, studentUserId);
                    }
                }
            }


            // Generate claims for the new user to support authentication
            var claims = ClaimsHelper.GenerateClaims(newUser.FirstName, newUser.Email);

            // Create an identity and sign in the user automatically after registration
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Redirect the user to the dashboard after successful registration
            return RedirectToPage("/Dashboard");
        }

        // Get classes for class search/dropdown
        public async Task<IActionResult> OnGetGetClassesAsync()
        {
            var classList = await _classService.GetAllClasses();
            return new JsonResult(classList.Select(c => new { name = c.Name }));
        }

        public async Task<IActionResult> OnGetGetTeamsAsync(string term, string className)
        {
            var course = await _classService.GetClassByNameAsync(className);

            // If the course doesn't exist, return an empty JSON array
            if (course == null)
            {
                return new JsonResult(Enumerable.Empty<object>());
            }

            var classId = course.Id;
            var allTeams = await _teamService.GetTeamsByClassId(classId);
            var filteredTeams = allTeams
                .Where(t => string.IsNullOrEmpty(term) || t.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(t => new { name = t.Name });

            return new JsonResult(filteredTeams);
        }
    }
}
