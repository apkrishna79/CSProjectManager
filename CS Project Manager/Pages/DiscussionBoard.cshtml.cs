/*
 * Prologue
 * Created By: Isabel Loney 
 * Date Created: 3/18/25
 * Last Revised By: Isabel Loney 
 * Date Revised: 3/26/25 
 * Purpose: Handles fetching and displaying discussion posts for a specific discussion board.
 * Preconditions: DiscussionPostService must be properly initialized and injected; DiscussionPost model must be correctly defined.
 * Postconditions: Retrieves and filters discussion posts associated with the board, ensuring replies are excluded.
 * Error and exceptions: ArgumentException (invalid board ID format)
 * Side effects: N/A
 * Invariants: _discussionPostService is always initialized with a valid instance; OnGetAsync always assigns a valid ObjectId to BoardId.
 * Other faults: N/A
 */

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class DiscussionBoardModel(DiscussionPostService discussionPostService, DiscussionBoardService discussionBoardService, ClassService classService, TeamService teamService) : PageModel
    {
        private readonly DiscussionPostService _discussionPostService = discussionPostService;
        private readonly DiscussionBoardService _discussionBoardService = discussionBoardService;
        private readonly ClassService _classService = classService;
        private readonly TeamService _teamService = teamService;

        public ObjectId BoardId { get; set; }
        [BindProperty]
        public string BoardName { get; set; }

        [BindProperty]
        public List<DiscussionPost> LinkedPosts { get; set; } = [];

        public async Task OnGetAsync(string boardId)
        {
            if (!ObjectId.TryParse(boardId, out ObjectId parsedBoardId))
            {
                throw new ArgumentException("Invalid project ID format");
            }
            BoardId = parsedBoardId;
            LinkedPosts = (await _discussionPostService.GetDiscussionPostsByBoardId(BoardId))
                .Where(post => !post.IsReply)
                .OrderByDescending(post => post.Timestamp)
                .ToList();

            var board = await _discussionBoardService.GetDiscussionBoardById(BoardId);

            BoardName = board.IsClassBoard ? (await _classService.GetClassByIdAsync(board.ClassId)).Name : (await _teamService.GetTeamByIdAsync(board.TeamId)).Name;
        }
    }
}
