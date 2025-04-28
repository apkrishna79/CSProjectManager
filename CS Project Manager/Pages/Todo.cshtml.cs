/*
 * Prologue
Created By: Jackson Wunderlich
Date Created: 3/13/25
Last Revised By: Dylan Sailors
Date Revised: 4/27/25
Purpose: Page displaying Team To-Do list and Personal To-Do list with the ability to add more tasks, mark task as complete, and remove a task
Preconditions: MongoDBService, other service instances properly initialized and injected; Requirement/Todo must be correctly defined
Postconditions: gets personal and team todo items and personal requirements from the database and shows them to the user
Error and exceptions: ArgumentNullException (required field empty), FormatException (invalid data input)
Side effects: N/A
Invariants: Todo list is always initialized; OnGet method always prepares an initial list
Other faults: N/A
*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using System.Security.Claims;

namespace CS_Project_Manager.Pages
{
    public class TodoModel : PageModel
    {
        private readonly TodoService _todoService;
        private readonly StudentUserService _studentUserService;
        private readonly TeamService _teamService;
        private readonly RequirementService _requirementService;
        private readonly ProjectService _projectService;

        public TodoModel(TodoService todoService, StudentUserService studentUserService, TeamService teamService, RequirementService requirementService, ProjectService projectService)
        {
            _todoService = todoService;
            _studentUserService = studentUserService;
            _teamService = teamService;
            _requirementService = requirementService;
            _projectService = projectService;
        }

        [BindProperty]
        public TodoItem NewTodo { get; set; } = new TodoItem { ItemName = string.Empty };
        public List<TodoItem> PersonalTodo { get; set; } = new();
        public List<TodoItem> TeamTodo { get; set; } = new();

        [BindProperty]
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();

        public List<Project> Projects { get; set; } = new List<Project>();
        public ObjectId UserId { get; set; }

        [BindProperty]
        public string PersonalTagFilter { get; set; } = "All";

        [BindProperty]
        public string TeamTagFilter { get; set; } = "All";

        public List<string> TagOptions { get; set; } = new List<string>
        {
            "All",
            "No tag",
            "General",
            "Code",
            "Documentation",
            "Brainstorm",
            "Bug Fix",
            "Other"
        };

        public async Task<IActionResult> OnGetAsync(string personalTagFilter = "All", string teamTagFilter = "All")
        {
            var user = await _studentUserService.GetUserByEmailAsync(User.FindFirstValue(ClaimTypes.Email));
            if (user == null) return RedirectToPage("/Login");

            PersonalTagFilter = personalTagFilter;
            TeamTagFilter = teamTagFilter;
            UserId = user.Id;
            var teamIds = (await _teamService.GetTeamsByStudentIdAsync(UserId)).ConvertAll(t => t.Id);
            Requirements = await _requirementService.GetRequirementsByUserIdAsync(UserId);
            var userTeams = await _teamService.GetTeamsByStudentIdAsync(UserId);

            // get all projects a user is currently part of
            foreach (var team in userTeams)
            {
                var projects = await _projectService.GetProjectsByTeamIdAsync(team.Id);
                foreach (var project in projects)
                {
                    Projects.Add(project);
                }
            }

            Requirements = Requirements
                .OrderBy(r => r.RequirementID ?? int.MaxValue)
                .ToList();

            // Fetch personal tasks and apply filter
            var allPersonalTodos = await _todoService.GetTodoByUserIdAsync(UserId);
            PersonalTodo = allPersonalTodos
                .Where(t => !t.IsTeamItem)
                .Where(t => PersonalTagFilter == "All" || t.Tag == PersonalTagFilter)
                .OrderBy(t => t.ItemComplete)
                .ToList();

            // Fetch team tasks and apply filter
            TeamTodo = new List<TodoItem>();
            foreach (var teamId in teamIds)
            {
                var todoItems = await _todoService.GetTodoByTeamIdAsync(teamId);
                TeamTodo.AddRange(todoItems);
            }

            TeamTodo = TeamTodo
                .Where(t => TeamTagFilter == "All" || t.Tag == TeamTagFilter)
                .OrderBy(t => t.ItemComplete)
                .ToList();

            return Page();
        }

        // Add a new task
        public async Task<IActionResult> OnPostAddAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTodo.ItemName))
            {
                ModelState.AddModelError("NewTodo.item_name", "The todo name cannot be empty.");
                return RedirectToPage();
            }

            var user = await _studentUserService.GetUserByEmailAsync(User.FindFirstValue(ClaimTypes.Email));
            if (user == null) return RedirectToPage("/Login");

            NewTodo.AssocUserId = user.Id;
            if (string.IsNullOrEmpty(NewTodo.Tag))
            {
                NewTodo.Tag = "No tag";
            }
            if (NewTodo.IsTeamItem)
            {
                var team = (await _teamService.GetTeamsByStudentIdAsync(user.Id)).FirstOrDefault();
                if (team != null)
                {
                    NewTodo.AssocTeamId = team.Id;
                }
            }

            await _todoService.AddTodoAsync(NewTodo);

            return RedirectToPage(new { personalTagFilter = PersonalTagFilter, teamTagFilter = TeamTagFilter });
        }

        // Toggle the completion status of a task
        public async Task<IActionResult> OnPostToggleCompleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return RedirectToPage(new { personalTagFilter = PersonalTagFilter, teamTagFilter = TeamTagFilter });
            }

            var todo = await _todoService.GetTodoByIdAsync(objectId);
            if (todo != null)
            {
                todo.ItemComplete = !todo.ItemComplete;
                await _todoService.UpdateTodoAsync(todo);
            }

            return RedirectToPage(new { personalTagFilter = PersonalTagFilter, teamTagFilter = TeamTagFilter });
        }

        // Remove a task
        public async Task<IActionResult> OnPostRemoveAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return RedirectToPage(new { personalTagFilter = PersonalTagFilter, teamTagFilter = TeamTagFilter });
            }

            await _todoService.RemoveTodoAsync(objectId);
            return RedirectToPage(new { personalTagFilter = PersonalTagFilter, teamTagFilter = TeamTagFilter });
        }

        // Update a task's tag
        public async Task<IActionResult> OnPostUpdateTagAsync(string id, string tag)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return RedirectToPage(new { personalTagFilter = PersonalTagFilter, teamTagFilter = TeamTagFilter });
            }
            var todo = await _todoService.GetTodoByIdAsync(objectId);
            if (todo != null)
            {
                todo.Tag = tag;
                await _todoService.UpdateTodoAsync(todo);
            }
            return RedirectToPage(new { personalTagFilter = PersonalTagFilter, teamTagFilter = TeamTagFilter });
        }

        // Filter personal tasks only
        public IActionResult OnPostFilterPersonalAsync()
        {
            return RedirectToPage(new { personalTagFilter = PersonalTagFilter, teamTagFilter = TeamTagFilter });
        }

        // Filter team tasks only
        public IActionResult OnPostFilterTeamAsync()
        {
            return RedirectToPage(new { personalTagFilter = PersonalTagFilter, teamTagFilter = TeamTagFilter });
        }

        // Handles filtering by tag
        public IActionResult OnPostFilterAsync()
        {
            return RedirectToPage(new { personalTagFilter = PersonalTagFilter, teamTagFilter = TeamTagFilter });
        }

        // Update a requirement
        public async Task<IActionResult> OnPostUpdateReqAsync(ObjectId id, ObjectId projectId)
        {
            var existingRequirement = await _requirementService.GetRequirementByIdAsync(id);
            if (existingRequirement == null)
            {
                return NotFound();
            }
            foreach (var req in Requirements)
            {
                Console.WriteLine(req.Id.ToString());
            }
            var updatedRequirement = Requirements.FirstOrDefault(r => r.Id == id);
            if (updatedRequirement != null)
            {
                existingRequirement.RequirementID = updatedRequirement.RequirementID;
                existingRequirement.Description = updatedRequirement.Description;
                existingRequirement.StoryPoints = updatedRequirement.StoryPoints;
                existingRequirement.Priority = updatedRequirement.Priority;
                existingRequirement.SprintNo = updatedRequirement.SprintNo;

                await _requirementService.UpdateRequirementAsync(existingRequirement);
            }
            Requirements = (await _requirementService.GetRequirementsByUserIdAsync(UserId))
                .OrderBy(r => r.RequirementID ?? int.MaxValue)
                .ToList();
            return RedirectToPage(new { personalTagFilter = PersonalTagFilter, teamTagFilter = TeamTagFilter });
        }

        public async Task<IActionResult> OnPostRemoveReqAsync(ObjectId id, ObjectId projectId)
        {
            var requirement = await _requirementService.GetRequirementByIdAsync(id);
            if (requirement == null)
            {
                return NotFound();
            }
            await _requirementService.RemoveRequirementAsync(id);
            Requirements = (await _requirementService.GetRequirementsByUserIdAsync(UserId))
                .OrderBy(r => r.RequirementID ?? int.MaxValue)
                .ToList();
            return RedirectToPage(new { personalTagFilter = PersonalTagFilter, teamTagFilter = TeamTagFilter });
        }
    }
}