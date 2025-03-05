using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ProjectService _projectService;
        private readonly TeamService _teamService;
        private readonly StudentUserService _userService;
        // private readonly ClassService _classService;

        // bound property for list of teams and projects
        [BindProperty]
        public List<Team> Teams { get; set; }

        public List<Project> Projects { get; set; }

        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(ILogger<DashboardModel> logger, ProjectService projectService, TeamService teamService, StudentUserService userService)
        {
            _logger = logger;
            _teamService = teamService;
            _userService = userService;
            // _classService = classService;
            _projectService = projectService;
            Teams = new List<Team>();
            Projects = new List<Project>();
        }

        public async void OnGetAsync()
        {
            var username = User.Identity.Name;
            var userObj = await _userService.GetUserByUsernameAsync(username);
            Teams = await _teamService.GetTeamsByStudentIdAsync(userObj.Id);
            foreach (var team in Teams)
            {
                var projects = await _projectService.GetProjectsByTeamIdAsync(team.Id);
                foreach (var project in projects)
                {
                    Projects.Add(project);
                }
            }
            // foreach (var project in Projects) { Console.WriteLine(project.project_name); }
        }
    }
}
