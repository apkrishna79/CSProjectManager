/*
 * Prologue
Created By: Jackson Wunderlich
Date Created: 3/13/25
Last Revised By: Dylan Sailors
Date Revised: 3/15/25
Purpose: Page displaying Team To-Do list and Personal To-Do list with the ability to add more tasks, mark task as complete, and remove a task
Preconditions: 
Postconditions: 
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

namespace CS_Project_Manager.Pages
{
    public class TodoModel : PageModel
    {
        private readonly TodoService _todoService;
        private readonly StudentUserService _studentUserService;
        private readonly TeamService _teamService;

        public TodoModel(TodoService todoService, StudentUserService studentUserService, TeamService teamService)
        {
            _todoService = todoService;
            _studentUserService = studentUserService;
            _teamService = teamService;
        }

        [BindProperty]
        public Todo NewTodo { get; set; } = new Todo { ItemName = string.Empty };
        public List<Todo> PersonalTodo { get; set; } = new();
        public List<Todo> TeamTodo { get; set; } = new();
        public ObjectId UserId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _studentUserService.GetUserByUsernameAsync(User.Identity.Name);
            if (user == null) return RedirectToPage("/Login");

            UserId = user.Id;
            var teamIds = (await _teamService.GetTeamsByStudentIdAsync(UserId)).ConvertAll(t => t.Id);

            // Fetch personal and team tasks using the TodoService
            PersonalTodo = (await _todoService.GetTodoByUserIdAsync(UserId))
                .Where(t => !t.IsTeamItem)
                .OrderBy(t => t.ItemComplete)
                .ToList();

            foreach (var team in teamIds)
            {
                var todoItems = await _todoService.GetTodoByTeamIdAsync(team);
                foreach (var todoItem in todoItems)
                {
                    TeamTodo.Add(todoItem);
                }
            }
            TeamTodo.OrderBy(t => t.ItemComplete);

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

            var user = await _studentUserService.GetUserByUsernameAsync(User.Identity.Name);
            if (user == null) return RedirectToPage("/Login");

            NewTodo.AssocUserId = user.Id;
            if (NewTodo.IsTeamItem)
            {
                var team = (await _teamService.GetTeamsByStudentIdAsync(user.Id)).FirstOrDefault();
                if (team != null)
                {
                    NewTodo.AssocTeamId = team.Id;
                }
            }

            await _todoService.AddTodoAsync(NewTodo);

            return RedirectToPage();
        }

        // Toggle the completion status of a task
        public async Task<IActionResult> OnPostToggleCompleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return RedirectToPage();
            }

            var todo = await _todoService.GetTodoByIdAsync(objectId);
            if (todo != null)
            {
                todo.ItemComplete = !todo.ItemComplete;
                await _todoService.UpdateTodoAsync(todo);
            }

            return RedirectToPage();
        }

        // Remove a task
        public async Task<IActionResult> OnPostRemoveAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                return RedirectToPage();
            }

            await _todoService.RemoveTodoAsync(objectId);

            return RedirectToPage();
        }

    }
}
