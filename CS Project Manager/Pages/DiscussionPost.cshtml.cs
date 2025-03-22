using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class DiscussionPostModel : PageModel
    {
        ObjectId PostId { get; set; }
        public void OnGet(string postId)
        {
            if (!ObjectId.TryParse(postId, out ObjectId parsedPostId))
            {
                throw new ArgumentException("Invalid project ID format");
            }
            PostId = parsedPostId;
        }
    }
 
}
