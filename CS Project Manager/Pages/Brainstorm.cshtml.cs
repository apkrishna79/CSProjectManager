using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.Security.Claims;

namespace CS_Project_Manager.Pages
{
    public class BrainstormModel : PageModel
    {
        private readonly BrainstormService _brainstormService;
        private readonly ProjectService _projectService;
        private readonly StudentUserService _studentUserService;

        [BindProperty]
        public List<BrainstormItem> BrainstormItems { get; set; } = new List<BrainstormItem>();
        public BrainstormItem NewBrainstormItem { get; set; } = new BrainstormItem
        {
            Description = string.Empty
        };

        [BindProperty(SupportsGet = true)]
        public ObjectId UserId { get; set; }
        public ObjectId ProjectId { get; set; }
        public List<StudentUser> Upvotes = new List<StudentUser>();
        public List<StudentUser> Downvotes = new List<StudentUser>();

        public BrainstormModel(BrainstormService brainstormService, ProjectService projectService, StudentUserService studentUserService)
        {
            _brainstormService = brainstormService;
            _projectService = projectService;
            _studentUserService = studentUserService;
        }

        public async Task<IActionResult> OnGetAsync(string projectId)
        {
            if (!ObjectId.TryParse(projectId, out ObjectId parsedProjectId))
            {
                throw new ArgumentException("Invalid project ID format");
            }

            ProjectId = parsedProjectId;

            BrainstormItems = await _brainstormService.GetBrainstormItemsByProjectIdAsync(parsedProjectId);
            return Page();

        }

        public async Task<IActionResult> OnPostAddAsync(ObjectId projectId)
        {
            if (string.IsNullOrWhiteSpace(NewBrainstormItem.Description))
            {
                ModelState.AddModelError("NewBrainstormItem.Description", "Description cannot be empty.");
            }
            if (!ModelState.IsValid)
            {
                ProjectId = projectId;
                BrainstormItems = await _brainstormService.GetBrainstormItemsByProjectIdAsync(projectId);
                return Page();
            }

            // get user by user ID
            // set user ID to the CreatedBy attr

            ProjectId = projectId;
            NewBrainstormItem.AssocProjectId = ProjectId;
            await _brainstormService.AddBrainstormItemAsync(NewBrainstormItem);

            return RedirectToPage(new { projectId = ProjectId.ToString() });
        }
    }
}