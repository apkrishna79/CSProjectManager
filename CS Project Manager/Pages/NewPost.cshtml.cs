using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class NewPostModel : PageModel
    {
        public ObjectId BoardId { get; set; }
        public void OnGet(string boardId)
        {
            if (!ObjectId.TryParse(boardId, out ObjectId parsedBoardId))
            {
                throw new ArgumentException("Invalid project ID format");
            }
            BoardId = parsedBoardId;
        }
    }
}
