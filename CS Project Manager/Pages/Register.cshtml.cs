using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using CS_Project_Manager.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IMongoCollection<Class> _classes;
        private readonly IMongoCollection<Team> _teams;
        private readonly StudentUserService _studentUserService;

        public RegisterModel(IMongoClient client, StudentUserService studentUserService)
        {
            var database = client.GetDatabase("CSProMan");
            _classes = database.GetCollection<Class>("Classes");
            _teams = database.GetCollection<Team>("Teams");
            _studentUserService = studentUserService;
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
        public List<string> Groups { get; set; } = [];

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
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
            var studentUserId = (await _studentUserService.GetUserByUsernameAsync(Username)).Id;

            foreach (var className in EnrolledClasses)
            {
                // Check if the class already exists
                var existingClass = await _classes.Find(c => c.Name == className).FirstOrDefaultAsync();
                if (existingClass == null)
                {
                    var newClass = new Class
                    {
                        Name = className,
                        EnrolledStudents = new List<ObjectId> { studentUserId }
                    };
                    // Insert new class since it doesn't exist
                    await _classes.InsertOneAsync(newClass);
                }
                else
                {
                    // If class exists, ensure the student is added to its EnrolledStudents list
                    if (!existingClass.EnrolledStudents.Contains(studentUserId))
                    {
                        var update = Builders<Class>.Update.AddToSet(c => c.EnrolledStudents, studentUserId);
                        await _classes.UpdateOneAsync(c => c.Name == className, update);
                    }
                }
            }

            return RedirectToPage("/Dashboard");
        }

        public async Task<IActionResult> OnGetGetClassesAsync()
        {
            var classList = await _classes.Find(_ => true).ToListAsync();
            return new JsonResult(classList.Select(c => new { name = c.Name }));
        }

        public async Task<IActionResult> OnGetGetTeamsAsync([FromQuery] List<string> classNames)
        {
            Console.WriteLine("Recieved GetClasses request...");
            var teamsList = await _teams.Find(t => classNames.Contains(t.AssociatedClass)).ToListAsync();
            return new JsonResult(teamsList.Select(t => new { id = t.Id, name = t.Name }));
        }


    }
}
