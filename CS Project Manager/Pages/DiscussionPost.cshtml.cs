/*
 * Prologue
 * Created By: Isabel Loney
 * Date Created: 3/22/25
 * Last Revised By: Isabel Loney
 * Date Revised: 3/27/25
 * Purpose: Handle discussion post interactions, including retrieving posts, fetching replies, and submitting new replies.
 * Preconditions: DiscussionPostService and StudentUserService instances must be properly initialized and injected; DiscussionPost model must be correctly defined.
 * Postconditions: Can retrieve discussion posts, fetch replies recursively, and allow users to submit new replies.
 * Error and exceptions: ArgumentException (invalid post ID format), BadRequest (empty reply content), potential database retrieval failures.
 * Side effects: Updates parent discussion post's ReplyIds list when a new reply is posted.
 * Invariants: _discussionPostService and _studentUserService are always initialized with valid instances.
 * Other faults: N/A
 */

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class DiscussionPostModel(DiscussionPostService discussionPostService, StudentUserService studentUserService) : PageModel
    {
        private readonly DiscussionPostService _discussionPostService = discussionPostService;
        private readonly StudentUserService _studentUserService = studentUserService;
        [BindProperty]
        public ObjectId PostId { get; set; }
        [BindProperty]
        public DiscussionPost Post {  get; set; }
        [BindProperty]
        public List<DiscussionPost> Replies { get; set; } = new();
        public async Task OnGetAsync(string postId)
        {
            if (!ObjectId.TryParse(postId, out ObjectId parsedPostId))
            {
                throw new ArgumentException("Invalid post ID format");
            }
            PostId = parsedPostId;
            Post = await _discussionPostService.GetDiscussionPostByIdAsync(PostId);

            Replies = await _discussionPostService.GetRepliesRecursivelyAsync(Post.ReplyIds);
        }

        public async Task<string> GetAuthorNameAsync(ObjectId posterId)
        {
            var user = await _studentUserService.GetUserByIdAsync(posterId);
            return user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";
        }

        public async Task<string> GetAuthorUsernameAsync(ObjectId posterId)
        {
            var user = await _studentUserService.GetUserByIdAsync(posterId);
            return user != null ? user.Username : "";
        }

        public async Task<IActionResult> OnPostPostReplyAsync(ObjectId immediateParentPostId, ObjectId headPostId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Reply content cannot be empty.");
            }

            var name = User.Identity.Name;
            var userObj = await _studentUserService.GetUserByUsernameAsync(name);

            var newReply = new DiscussionPost
            {
                HeadPostId = headPostId,
                PosterId = userObj.Id,
                Content = content,
                Timestamp = DateTime.UtcNow,
                IsReply = true,
                ReplyIds = new List<ObjectId>(),  // New reply has no replies initially
                BoardId = (await _discussionPostService.GetDiscussionPostByIdAsync(headPostId)).BoardId
            };

            // Save the new reply to get its ID
            await _discussionPostService.CreateDiscussionPostAsync(newReply);

            // Now, update the IMMEDIATE PARENT post to include this new reply's ID
            var parentPost = await _discussionPostService.GetDiscussionPostByIdAsync(immediateParentPostId);
            if (parentPost != null)
            {
                parentPost.ReplyIds.Add(newReply.Id);
                await _discussionPostService.UpdateDiscussionPostAsync(parentPost);
            }

            return new JsonResult(new { success = true });
        }

        public async Task<DiscussionPost> GetDiscussionPostByIdAsync(ObjectId postId)
        {
            return await _discussionPostService.GetDiscussionPostByIdAsync(postId);
        }
        public async Task<IActionResult> OnPostDeletePostAsync(ObjectId postId)
        {
            var post = await _discussionPostService.GetDiscussionPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound("Post not found.");
            }

            // Ensure the current user is the author
            var user = await _studentUserService.GetUserByUsernameAsync(User.Identity.Name);
            if (post.PosterId != user.Id)
            {
                return Forbid(); // User is not allowed to delete this post
            }

            string redirectUrl;

            if (post.IsReply)
            {
                // If it's a reply, redirect to the main discussion post
                redirectUrl = Url.Page("/DiscussionPost", new { postId = post.HeadPostId.ToString() });
            }
            else
            {
                // If it's a main post, redirect to the discussion board
                redirectUrl = Url.Page("/DiscussionBoard", new { boardId = post.BoardId.ToString() });
            }

            await _discussionPostService.DeleteDiscussionPostAsync(postId);

            // Return JSON containing redirect URL
            return new JsonResult(new { redirectUrl });
        }


    }

}
