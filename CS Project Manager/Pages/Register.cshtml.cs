using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using CS_Project_Manager.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly StudentUserService _studentUserService;
        private readonly ClassService _classService;
        private readonly TeamService _teamService;

        public RegisterModel(StudentUserService studentUserService, ClassService classService, TeamService teamService)
        {
            _studentUserService = studentUserService;
            _classService = classService;
            _teamService = teamService;
        }

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
                return Page();
            }

            // Check if an account already exists with the given username
            var existingUser = await _studentUserService.GetUserByUsernameAsync(Username);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Username is already in use.");
                return Page(); // Display error if email is already in use
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

            // Map classes to ObjectIds
            var classIdMap = new Dictionary<string, ObjectId>();

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
                }
                else
                {
                    classId = existingClass.Id;

                    if (!existingClass.EnrolledStudents.Contains(studentUserId))
                    {
                        await _classService.AddStudentToClassAsync(classId, studentUserId);
                    }
                }

                classIdMap[className] = classId;
            }

            // Handle team assignment
            foreach (var teamSelection in Teams)
            {
                if (!teamSelection.Contains("(") || !teamSelection.Contains(")"))
                    continue; // Skip invalid formats

                var teamName = teamSelection.Split(" (")[0]; // Extract just the team name
                var className = teamSelection.Split(" (")[1].TrimEnd(')'); // Extract the class name

                if (!classIdMap.TryGetValue(className, out ObjectId classId))
                {
                    continue; // Skip if the class isn't valid
                }

                var existingTeam = await _teamService.GetTeamByNameAndClassId(teamName, classId);

                if (existingTeam == null)
                {
                    var newTeam = new Team
                    {
                        Name = teamName,
                        AssociatedClass = classId,
                        Members = new List<ObjectId> { studentUserId }
                    };
                    await _teamService.CreateTeamAsync(newTeam);
                }
                else
                {
                    if (!existingTeam.Members.Contains(studentUserId))
                    {
                        await _teamService.AddStudentToTeamAsync(existingTeam.Id, studentUserId);
                    }
                }
            }

            return RedirectToPage("/Dashboard");
        }



        public async Task<IActionResult> OnGetGetClassesAsync()
        {
            var classList = await _classService.GetAllClasses();
            return new JsonResult(classList.Select(c => new { name = c.Name }));
        }

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
