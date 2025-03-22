using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace CS_Project_Manager.Pages
{
    public class NewPostModel(StudentUserService studentUserService, DiscussionPostService discussionPostService) : PageModel
    {
        private readonly StudentUserService _studentUserService = studentUserService;
        private readonly DiscussionPostService _discussionPostService = discussionPostService;
        [BindProperty]
        public ObjectId BoardId { get; set; }

        [BindProperty]
        [Required]
        public required string Title { get; set; }

        [BindProperty]
        [Required]
        public required string Body { get; set; }

        public void OnGet(string boardId)
        {
            if (!ObjectId.TryParse(boardId, out ObjectId parsedBoardId))
            {
                throw new ArgumentException("Invalid project ID format");
            }
            BoardId = parsedBoardId;
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var newPost = new DiscussionPost
            {
                BoardId = this.BoardId,
                PosterId = (await _studentUserService.GetUserByUsernameAsync(User.Identity.Name)).Id,
                Title = this.Title,
                Content = Body,
                Timestamp = DateTime.UtcNow,
                HeadPostId = ObjectId.Empty,
                ImmediateParentPostId = ObjectId.Empty,
                IsReply = false
            };

            await _discussionPostService.CreateDiscussionPostAsync(newPost);
            return RedirectToPage("/DiscussionPost", new { postId = newPost.Id.ToString() });
        }
    }
}
