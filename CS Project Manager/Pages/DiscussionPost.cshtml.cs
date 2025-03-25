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

    }

}
