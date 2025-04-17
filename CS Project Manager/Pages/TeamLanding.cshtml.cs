using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class TeamLandingModel : PageModel
    {
        [BindProperty]
        public ObjectId TeamId { get; set; }

        public void OnGet(string teamId)
        {
            if (!ObjectId.TryParse(teamId, out ObjectId parsedTeamId))
            {
                throw new ArgumentException("Invalid Team ID format");
            }
            TeamId = parsedTeamId;
        }
    }
}
