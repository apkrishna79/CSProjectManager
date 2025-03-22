using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.Linq;
using System.Threading.Tasks;

namespace CS_Project_Manager.Pages
{
    public class SelectDiscussionBoardModel(DiscussionBoardService boardService, ClassService classService, StudentUserService studentUserService, TeamService teamService) : PageModel
    {
        private readonly DiscussionBoardService _boardService = boardService;
        private readonly ClassService _classService = classService;
        private readonly StudentUserService _studentUserService = studentUserService;
        private readonly TeamService _teamService = teamService;

        public List<(string label, ObjectId assocBoardId)> TeamDisplayData { get; set; } = new();
        public List<(string label, ObjectId assocBoardId)> ClassDisplayData { get; set; } = new();

        public async Task OnGet()
        {
            var name = User.Identity.Name;
            var userObj = await _studentUserService.GetUserByUsernameAsync(name);

            var classes = await _classService.GetClassesForStudentAsync(userObj.Id);
            var classIdToNameMap = classes.ToDictionary(c => c.Id, c => c.Name);

            // Class Discussion Boards
            var classIds = classes.Select(c => c.Id).ToList();
            var availableClassBoards = await _boardService.GetDiscussionBoardsByClassIdsAsync(classIds);
            ClassDisplayData = [.. classes.Select(course =>(
                course.Name,
                availableClassBoards.FirstOrDefault(b => b.ClassId == course.Id).Id
            ))];

            // Team discussion boards
            var teams = await _teamService.GetTeamsByStudentIdAsync(userObj.Id);
            var teamIds = teams.Select(team => team.Id).ToList();
            var availableTeamBoards = await _boardService.GetDiscussionBoardsByTeamIdsAsync(teamIds);

            TeamDisplayData = [.. teams.Select(team => (
                $"{team.Name} ({classIdToNameMap.GetValueOrDefault(team.AssociatedClass)})",
                availableTeamBoards.FirstOrDefault(b => b.TeamId == team.Id).Id
            ))];
        }
    }
}
