using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CS_Project_Manager.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;

        private readonly ProjectService _projectService;

        public DashboardModel(ILogger<DashboardModel> logger, ProjectService projectService)
        {
            _logger = logger;
            _projectService = projectService;
        }

        public void OnGetAsync()
        {

        }
    }
}
