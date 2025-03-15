/*
 * Prologue
Created By: Jackson Wunderlich
Date Created: 3/13/25
Last Revised By: Jackson Wunderlich
Date Revised: 3/13/25
Purpose: 
Preconditions: 
Postconditions: 
Error and exceptions: ArgumentNullException (required field empty), FormatException (invalid data input)
Side effects: N/A
Invariants: Todo list is always initialized; OnGet method always prepares an initial list
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class TodoModel : PageModel
    {
        private readonly RequirementService _requirementService;
        private readonly TodoService _todoService;
        private readonly ProjectService _projectService;

        [BindProperty]
        public List<Todo> TodoItems { get; set; } = new List<Todo>();

        [BindProperty]
        public Todo NewTodo { get; set; } = new Todo
        {
            item_name = string.Empty,
            item_complete = false,
            is_team_item = false
        };

        [BindProperty(SupportsGet = true)]
        public ObjectId ProjectId { get; set; }

        public TodoModel(RequirementService requirementService, ProjectService projectService, TodoService todoService)
        {
            _requirementService = requirementService;
            _projectService = projectService;
            _todoService = todoService;
        }
        /*
        public async Task OnGetAsync(string projectId)
        {
            if (!ObjectId.TryParse(projectId, out ObjectId parsedProjectId))
            {
                throw new ArgumentException("Invalid project ID format");
            }
            ProjectId = parsedProjectId;
            TodoItems = await _todoService.GetTodoByUserIdAsync(parsedProjectId);
        }

        // Add a new requirement
        public async Task<IActionResult> OnPostAddAsync(ObjectId projectId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            ProjectId = projectId;
            var project = await _projectService.GetProjectById(ProjectId);
            if (project == null)
            {
                return NotFound();
            }
            NewTodo.AssocUserId = ProjectId;
            await _todoService.AddTodoAsync(NewTodo);
            return RedirectToPage(new { projectId = ProjectId.ToString() });
        }

        // Update an existing requirement
        public async Task<IActionResult> OnPostUpdateAsync(ObjectId id, ObjectId projectId)
        {
            var existingTodo = await _todoService.GetTodoByIdAsync(id);
            if (existingTodo == null)
            {
                return NotFound();
            }
            var updatedRequirement = TodoItems.FirstOrDefault(r => r.Id == id);
            if (updatedRequirement != null)
            {
                existingTodo.is_team_item = updatedRequirement.is_team_item;
                existingTodo.item_name = updatedRequirement.item_name;
                existingTodo.item_complete = updatedRequirement.item_complete;
                existingTodo.AssocTeamId = updatedRequirement.AssocTeamId;
                existingTodo.AssocUserId = updatedRequirement.AssocUserId;
                await _todoService.UpdateTodoAsync(existingTodo);
            }
            return RedirectToPage(new { projectId = projectId.ToString() });
        }

        // Remove an existing requirement
        public async Task<IActionResult> OnPostRemoveAsync(ObjectId id, ObjectId projectId)
        {
            var requirement = await _requirementService.GetRequirementByIdAsync(id);
            if (requirement == null)
            {
                return NotFound();
            }
            await _requirementService.RemoveRequirementAsync(id);
            return RedirectToPage(new { projectId = projectId.ToString() });
        }
        */
    }
}