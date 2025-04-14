/*
 * Prologue
Created By: Ginny Ke
Date Created: 4/14/25
Last Revised By: Ginny Ke
Date Revised: 4/14/25
Purpose: Create, update, remove sprint board items for an associated project
Preconditions: MongoDBService, ProjectService, SprintBoardService, StudentUserService instances properly initialized and injected; Project, StudentUser, SprintBoardItem models must be correctly defined
Postconditions: Can add, remove, and update sprint board items associated with project
Error and exceptions: ArgumentNullException (required field empty)
Side effects: N/A
Invariants: _projectService, _sprintBoardService, _studentUserService fields are always initialized with valid instance, OnPostAsync method always returns an IActionResult
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
    public class SprintBoardModel : PageModel
    {
        private readonly SprintBoardService _sprintBoardService;
        private readonly ProjectService _projectService;
        private readonly StudentUserService _studentUserService;

        [BindProperty]
        public List<SprintBoardItem> SprintBoardItems { get; set; } = new();

        [BindProperty]
        public SprintBoardItem NewSprintBoardItem { get; set; } = new()
        {
            Description = string.Empty,
            Category = "WentWell",
            SprintNumber = 1
        };

        [BindProperty(SupportsGet = true)]
        public ObjectId ProjectId { get; set; }
        public ObjectId UserId { get; set; }
        public Dictionary<ObjectId, string> UserFullNames { get; set; } = new();

        public SprintBoardModel(SprintBoardService sprintBoardService, ProjectService projectService, StudentUserService studentUserService)
        {
            _sprintBoardService = sprintBoardService;
            _projectService = projectService;
            _studentUserService = studentUserService;
        }

        public async Task<IActionResult> OnGetAsync(ObjectId projectId, int? sprintNumber)
        {
            ProjectId = projectId;
            await LoadSprintBoardItemsAsync(ProjectId, sprintNumber);
            return Page();
        }

        private async Task LoadSprintBoardItemsAsync(ObjectId projectId, int? sprintNumber = null)
        {
            var allItems = await _sprintBoardService.GetItemsByProjectIdAsync(projectId);
            SprintBoardItems = sprintNumber.HasValue ? allItems.Where(i => i.SprintNumber == sprintNumber.Value).ToList() : allItems;
            await PopulateUserFullNamesAsync();
        }

        public async Task<IActionResult> OnPostAddAsync(ObjectId projectId)
        {
            if (string.IsNullOrWhiteSpace(NewSprintBoardItem.Description))
            {
                ModelState.AddModelError("NewSprintBoardItem.Description", "Description cannot be empty.");
            }

            if (!ModelState.IsValid)
            {
                ProjectId = projectId;
                await LoadSprintBoardItemsAsync(ProjectId);
                return Page();
            }

            var user = await _studentUserService.GetUserByEmailAsync(User.FindFirstValue(ClaimTypes.Email));
            UserId = user.Id;

            NewSprintBoardItem.CreatedBy = UserId;
            NewSprintBoardItem.AssocProjectId = projectId;

            await _sprintBoardService.AddItemAsync(NewSprintBoardItem);
            ModelState.Clear();
            NewSprintBoardItem = new SprintBoardItem { Description = string.Empty, Category = "WentWell", SprintNumber = 1 };
            await LoadSprintBoardItemsAsync(projectId);
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(ObjectId id, ObjectId projectId, string description, int sprintNumber)
        {
            var existingItem = await _sprintBoardService.GetItemByIdAsync(id);
            if (existingItem == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(description))
            {
                ModelState.AddModelError("description", "Description cannot be empty.");
                ProjectId = projectId;
                await LoadSprintBoardItemsAsync(ProjectId);
                return Page();
            }

            existingItem.Description = description;
            existingItem.SprintNumber = sprintNumber;
            await _sprintBoardService.UpdateItemAsync(existingItem);

            ProjectId = projectId;
            await LoadSprintBoardItemsAsync(ProjectId);

            // 🧹 Clear the NewSprintBoardItem to avoid pre-filling form on postback
            NewSprintBoardItem = new SprintBoardItem { Description = string.Empty, Category = "WentWell", SprintNumber = 1 };

            return Page();
        }


        public async Task<IActionResult> OnPostRemoveAsync(ObjectId id, ObjectId projectId)
        {
            var existingItem = await _sprintBoardService.GetItemByIdAsync(id);
            if (existingItem == null)
                return NotFound();

            await _sprintBoardService.RemoveItemAsync(id);
            return RedirectToPage(new { projectId });
        }

        private async Task PopulateUserFullNamesAsync()
        {
            var users = await _studentUserService.GetAllUsersAsync();
            foreach (var user in users)
            {
                UserFullNames[user.Id] = $"{user.FirstName} {user.LastName}";
            }
        }
    }
}
