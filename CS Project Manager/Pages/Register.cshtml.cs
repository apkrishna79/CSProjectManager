/*
 * Prologue
Created By: Isabel Loney
Date Created: 2/25/25
Last Revised By: Isabel Loney
Date Revised: 3/26/25
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
        [MaxLength(100)]
        public required string Username { get; set; }

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
        [EmailAddress]
        [MaxLength(255)]
        public string? ContactEmail { get; set; }

        [BindProperty]
        public List<string> EnrolledClasses { get; set; } = [];

        [BindProperty]
        public List<string> Teams { get; set; } = [];

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page(); // return page with validation errors
            }

            // Check if an account already exists with the given username
            var existingUser = await _studentUserService.GetUserByUsernameAsync(Username);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Username is already in use.");
                return Page(); // Display error if username is already in use
            }

            var newUser = new StudentUser
            {
                Username = Username,
                PasswordHash = PasswordHelper.HashPassword(Password),
                FirstName = FirstName,
                LastName = LastName,
                Email = ContactEmail,
            };

            await _studentUserService.CreateUserAsync(newUser);
            var studentUserId = newUser.Id;

            // Dictionary to map class names to their corresponding ObjectIdss
            var classIdMap = new Dictionary<string, ObjectId>();

            // Process each selected class
            foreach (var className in EnrolledClasses)
            {
                var existingClass = await _classService.GetClassByNameAsync(className);
                ObjectId classId;

                if (existingClass == null)
                {
                    // If the class does not exist, create a new one and enroll the student
                    var newClass = new Class
                    {
                        Name = className,
                        EnrolledStudents = new List<ObjectId> { studentUserId }
                    };
                    await _classService.CreateClassAsync(newClass);
                    classId = newClass.Id;

                    // create discussion board for new class
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

                    // If the student is not already enrolled in the class, add them
                    if (!existingClass.EnrolledStudents.Contains(studentUserId))
                    {
                        await _classService.AddStudentToClassAsync(classId, studentUserId);
                    }
                }

                // Store the class ID in the mapping dictionary for later use
                classIdMap[className] = classId;
            }

            // Process each selected team
            foreach (var teamSelection in Teams)
            {
                // Ensure the team selection is in the expected "Team Name (Class Name)" format
                if (!teamSelection.Contains("(") || !teamSelection.Contains(")"))
                    continue; // Skip invalid formats

                // Extract the team name and associated class name
                var teamName = teamSelection.Split(" (")[0];
                var className = teamSelection.Split(" (")[1].TrimEnd(')');

                // Check if the class exists in the previously mapped class IDs
                if (!classIdMap.TryGetValue(className, out ObjectId classId))
                {
                    continue; // Skip if the class isn't valid
                }

                // Check if the team already exists for the given class
                var existingTeam = await _teamService.GetTeamByNameAndClassId(teamName, classId);

                if (existingTeam == null)
                {
                    // If the team does not exist, create a new one and add the student
                    var newTeam = new Team
                    {
                        Name = teamName,
                        AssociatedClass = classId,
                        Members = [studentUserId]
                    };
                    await _teamService.CreateTeamAsync(newTeam);

                    // create discussion board for new class
                    await _discussionBoardService.CreateDiscussionBoardAsync(new DiscussionBoard
                    {
                        IsClassBoard = false,
                        ClassId = ObjectId.Empty,
                        TeamId = newTeam.Id
                    });
                }
                else
                {
                    // If the student is not already a member of the team, add them
                    if (!existingTeam.Members.Contains(studentUserId))
                    {
                        await _teamService.AddStudentToTeamAsync(existingTeam.Id, studentUserId);
                    }
                }
            }

            // Generate claims for the new user to support authentication
            var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, Username), // Claim for the user's name
                };

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

        // Get teams for team search/dropdown based on selected classes
        public async Task<IActionResult> OnGetGetTeamsForClassesAsync([FromQuery] string[] cs)
        {
            var classIdToName = await _classService.GetClassIdToName(cs);

            var teamsForSelectedClasses = await _teamService.GetTeamsByClassNames(classIdToName);

            var teamList = teamsForSelectedClasses.Select(t => new
            {
                name = $"{t.Name} ({classIdToName[t.AssociatedClass]})"
            }).ToList();

            return new JsonResult(teamList);
        }

    }
}
