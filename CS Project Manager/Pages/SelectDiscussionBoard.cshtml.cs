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

        List<DiscussionBoard> AvailableClassBoards = [];
        List<DiscussionBoard> AvailableTeamBoards = [];
        public List<Class> Classes = [];
        public List<Team> Teams = [];
        public List<(ObjectId Id, string label)> TeamDisplayData { get; set; } = new();

        public async Task OnGet()
        {
            var name = User.Identity.Name;
            var userObj = await _studentUserService.GetUserByUsernameAsync(name);

            Classes = await _classService.GetClassesForStudentAsync(userObj.Id);
            var classIdToNameMap = Classes.ToDictionary(c => c.Id, c => c.Name);

            // Class Discussion Boards
            var classIds = Classes.Select(c => c.Id).ToList();
            AvailableClassBoards = await _boardService.GetDiscussionBoardsByClassIdsAsync(classIds);

            Teams = await _teamService.GetTeamsByStudentIdAsync(userObj.Id);
            var teamIds = Teams.Select(team => team.Id).ToList();
            AvailableTeamBoards = await _boardService.GetDiscussionBoardsByTeamIdsAsync(teamIds);

            TeamDisplayData = [.. Teams.Select(team => (
                team.Id,
                $"{team.Name} ({classIdToNameMap.GetValueOrDefault(team.AssociatedClass, "Unknown Class")})"
            ))];
        }
    }
}
