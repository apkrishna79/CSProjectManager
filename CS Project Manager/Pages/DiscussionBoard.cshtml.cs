using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class DiscussionBoardModel(DiscussionPostService discussionPostService) : PageModel
    {
        private readonly DiscussionPostService _discussionPostService = discussionPostService;

        public ObjectId BoardId { get; set; }

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
                .ToList();
        }
    }
}
