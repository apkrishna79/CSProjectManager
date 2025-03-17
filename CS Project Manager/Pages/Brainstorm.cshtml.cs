/*
 * Prologue
Created By: Anakha Krishna
Date Created: 3/15/25
Last Revised By: Dylan Sailors
Date Revised: 3/16/25 - DS
Purpose: Create, update, remove brainstorm ideas for an associated project
Preconditions: MongoDBService, ProjectService, BrainstormService, StudentUserService instances properly initialized and injected; Project, StudentUser, BrainstormItem models must be correctly defined
Postconditions: Can add, remove, and update brainstorm ideas associated with project
Error and exceptions: ArgumentNullException (required field empty)
Side effects: N/A
Invariants: _projectService, _brainstormService, _studentUserService fields are always initialized with valid instance, OnPostAsync method always returns an IActionResult
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class BrainstormModel : PageModel
    {
        private readonly BrainstormService _brainstormService;
        private readonly ProjectService _projectService;
        private readonly StudentUserService _studentUserService;

        [BindProperty]
        public List<BrainstormItem> BrainstormItems { get; set; } = new List<BrainstormItem>();
        [BindProperty]
        public BrainstormItem NewBrainstormItem { get; set; } = new BrainstormItem
        {
            Description = string.Empty
        };

        [BindProperty(SupportsGet = true)]
        public ObjectId ProjectId { get; set; }
        public ObjectId UserId { get; set; }
        public Dictionary<ObjectId, string> UserFullNames { get; set; } = new Dictionary<ObjectId, string>();

        public BrainstormModel(BrainstormService brainstormService, ProjectService projectService, StudentUserService studentUserService)
        {
            _brainstormService = brainstormService;
            _projectService = projectService;
            _studentUserService = studentUserService;
        }

        // Helper method to load and sort brainstorm items
        private async Task LoadAndSortBrainstormItemsAsync(ObjectId projectId)
        {
            BrainstormItems = await _brainstormService.GetBrainstormItemsByProjectIdAsync(projectId);
            BrainstormItems = BrainstormItems
                .OrderByDescending(item => item.Upvotes.Count - item.Downvotes.Count)
                .ToList();
            await PopulateUserFullNamesAsync();
        }

        // Load BrainstormItems associated with project
        public async Task<IActionResult> OnGetAsync(ObjectId projectId)
        {
            ProjectId = projectId;
            await LoadAndSortBrainstormItemsAsync(ProjectId);
            return Page();
        }

        // Handle adding a new brainstorm idea
        public async Task<IActionResult> OnPostAddAsync(ObjectId projectId)
        {
            if (string.IsNullOrWhiteSpace(NewBrainstormItem.Description))
            {
                ModelState.AddModelError("NewBrainstormItem.Description", "Description cannot be empty.");
            }
            if (!ModelState.IsValid)
            {
                ProjectId = projectId;
                await LoadAndSortBrainstormItemsAsync(ProjectId);
                return Page();
            }
            var user = await _studentUserService.GetUserByUsernameAsync(User.Identity.Name);
            UserId = user.Id;
            NewBrainstormItem.CreatedBy = UserId;
            ProjectId = projectId;
            NewBrainstormItem.AssocProjectId = ProjectId;
            await _brainstormService.AddBrainstormItemAsync(NewBrainstormItem);
            ModelState.Clear();
            NewBrainstormItem = new BrainstormItem { Description = string.Empty };
            await LoadAndSortBrainstormItemsAsync(ProjectId);
            return Page();
        }


        // Handle updating an existing brainstorm idea
        public async Task<IActionResult> OnPostUpdateAsync(ObjectId id, ObjectId projectId)
        {
            var existingBrainstormItem = await _brainstormService.GetBrainstormItemByIdAsync(id);
            if (existingBrainstormItem == null)
            {
                return NotFound();
            }
            var updatedBrainstormItem = BrainstormItems.FirstOrDefault(b => b.Id == id);
            if (updatedBrainstormItem != null)
            {
                if (string.IsNullOrWhiteSpace(updatedBrainstormItem.Description))
                {
                    ModelState.AddModelError($"BrainstormItems[{BrainstormItems.IndexOf(updatedBrainstormItem)}].Description", "Description cannot be empty.");
                    ProjectId = projectId;
                    await LoadAndSortBrainstormItemsAsync(ProjectId);
                    return Page();
                }
                existingBrainstormItem.Description = updatedBrainstormItem.Description;
                await _brainstormService.UpdateBrainstormItemAsync(existingBrainstormItem);
            }
            ProjectId = projectId;
            await LoadAndSortBrainstormItemsAsync(ProjectId);
            return Page();
        }

        // Handle deleting an existing brainstorm idea
        public async Task<IActionResult> OnPostRemoveAsync(ObjectId id, ObjectId projectId)
        {
            var existingBrainstormItem = await _brainstormService.GetBrainstormItemByIdAsync(id);
            if (existingBrainstormItem == null)
            {
                return NotFound();
            }
            await _brainstormService.RemoveBrainstormItemAsync(id);
            await LoadAndSortBrainstormItemsAsync(projectId);
            return RedirectToPage(new { projectId });
        }

        // Handles dictionary UserFullNames to show the CreatedBy attribute of BrainstormItem as a First name and Last name instead of a StudentUser ObjectId
        private async Task PopulateUserFullNamesAsync()
        {
            var users = await _studentUserService.GetAllUsersAsync();
            foreach (var user in users)
            {
                UserFullNames[user.Id] = $"{user.FirstName} {user.LastName}";
            }
        }

        // Handles upvoting a brainstorm idea
        public async Task<IActionResult> OnPostUpvoteAsync(ObjectId id, ObjectId projectId)
        {
            var brainstormItem = await _brainstormService.GetBrainstormItemByIdAsync(id);
            if (brainstormItem == null)
                return NotFound();
            var user = await _studentUserService.GetUserByUsernameAsync(User.Identity.Name);
            var userId = user.Id;
            if (brainstormItem.Upvotes.Contains(userId))
            {
                brainstormItem.Upvotes.Remove(userId);
            }
            else
            {
                brainstormItem.Upvotes.Add(userId);
                brainstormItem.Downvotes.Remove(userId);
            }
            await _brainstormService.UpdateBrainstormItemAsync(brainstormItem);
            ModelState.Clear();
            await LoadAndSortBrainstormItemsAsync(projectId);
            return Page();
        }


        // Handles downvoting a brainstorm idea
        public async Task<IActionResult> OnPostDownvoteAsync(ObjectId id, ObjectId projectId)
        {
            var brainstormItem = await _brainstormService.GetBrainstormItemByIdAsync(id);
            if (brainstormItem == null)
                return NotFound();
            var user = await _studentUserService.GetUserByUsernameAsync(User.Identity.Name);
            var userId = user.Id;
            if (brainstormItem.Downvotes.Contains(userId))
            {
                brainstormItem.Downvotes.Remove(userId);
            }
            else
            {
                brainstormItem.Downvotes.Add(userId);
                brainstormItem.Upvotes.Remove(userId);
            }
            await _brainstormService.UpdateBrainstormItemAsync(brainstormItem);
            ModelState.Clear();
            await LoadAndSortBrainstormItemsAsync(ProjectId);
            return Page();
        }
    }
}
