/*
 * Prologue
 * Created By: Isabel Loney 
 * Date Created: 3/22/25
 * Last Revised By: Isabel Loney 
 * Date Revised: 3/26/25 
 * Purpose: Handle the creation of new discussion posts associated with a board.
 * Preconditions: StudentUserService and DiscussionPostService instances properly initialized and injected; 
 *               User must be authenticated; BoardId must be a valid ObjectId.
 * Postconditions: A new discussion post is created and stored in the database, then redirects to the post's page.
 * Error and exceptions: ArgumentException (invalid board ID format); ValidationException (missing or invalid input data).
 * Side effects: A new discussion post is added to the database.
 * Invariants: _studentUserService and _discussionPostService are always initialized with valid instances.
 * Other faults: N/A
 */

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
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; }


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
                IsReply = false,
                ReplyIds = []
            };

            await _discussionPostService.CreateDiscussionPostAsync(newPost);
            return RedirectToPage("/DiscussionPost", new { postId = newPost.Id.ToString() });
        }
    }
}
