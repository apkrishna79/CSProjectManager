/*
 * Prologue
Created By: Anakha Krishna
Date Created: 3/15/25
Last Revised By: Anakha Krishna
Date Revised: 3/16/25 - AK
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
        public List<StudentUser> Upvotes = new List<StudentUser>();
        public List<StudentUser> Downvotes = new List<StudentUser>();
        public Dictionary<ObjectId, string> UserFullNames { get; set; } = new Dictionary<ObjectId, string>();

        public BrainstormModel(BrainstormService brainstormService, ProjectService projectService, StudentUserService studentUserService)
        {
            _brainstormService = brainstormService;
            _projectService = projectService;
            _studentUserService = studentUserService;
        }
        
        // Load BrainstormItems associated with project
        public async Task<IActionResult> OnGetAsync(ObjectId projectId)
        {
            ProjectId = projectId;
            BrainstormItems = await _brainstormService.GetBrainstormItemsByProjectIdAsync(ProjectId);
            await PopulateUserFullNamesAsync();
            return Page();
        }

        // Handle adding a new brainstorm idea
        public async Task<IActionResult> OnPostAddAsync(ObjectId projectId)
        {
            if (string.IsNullOrWhiteSpace(NewBrainstormItem.Description))
            {
                ModelState.AddModelError("NewBrainstormItem.Description", "Description cannot be empty.");
            }
            
            // Reload brainstorm list after ModelError
            if (!ModelState.IsValid)
            {
                ProjectId = projectId;
                BrainstormItems = await _brainstormService.GetBrainstormItemsByProjectIdAsync(ProjectId);
                await PopulateUserFullNamesAsync();
                return Page();
            }

            var user = await _studentUserService.GetUserByUsernameAsync(User.Identity.Name);
            UserId = user.Id;
            NewBrainstormItem.CreatedBy = UserId;

            ProjectId = projectId;
            NewBrainstormItem.AssocProjectId = ProjectId;

            await _brainstormService.AddBrainstormItemAsync(NewBrainstormItem);
            BrainstormItems = await _brainstormService.GetBrainstormItemsByProjectIdAsync(ProjectId);

            await PopulateUserFullNamesAsync();
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
                    BrainstormItems = await _brainstormService.GetBrainstormItemsByProjectIdAsync(ProjectId);
                    await PopulateUserFullNamesAsync();
                    return Page();
                }
                existingBrainstormItem.Description = updatedBrainstormItem.Description;
                await _brainstormService.UpdateBrainstormItemAsync(existingBrainstormItem);
            }

            ProjectId = projectId;
            BrainstormItems = await _brainstormService.GetBrainstormItemsByProjectIdAsync(ProjectId);

            await PopulateUserFullNamesAsync();
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

            ProjectId = projectId;
            BrainstormItems = await _brainstormService.GetBrainstormItemsByProjectIdAsync(ProjectId);
            await PopulateUserFullNamesAsync();

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
    }
}