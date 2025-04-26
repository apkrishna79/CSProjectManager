using CS_Project_Manager.Services;
using MongoDB.Bson;

namespace CS_Project_Manager.Utilities
{
    public class ProjectDisplayHelper
    {
        // Calculate the progress for a project based on all requirements
        public static async Task CalculateProjectProgressAsync(ObjectId projectId, Dictionary<ObjectId, decimal> projectProgressDict, RequirementService requirementService)
        {
            var requirements = await requirementService.GetRequirementsByProjectIdAsync(projectId);
            if (requirements == null || !requirements.Any())
            {
                projectProgressDict[projectId] = 0;
                return;
            }
            decimal totalProgress = 0;
            int totalRequirements = requirements.Count;
            foreach (var requirement in requirements)
            {
                totalProgress += requirement.Progress ?? 0;
            }
            decimal overallProgress = totalRequirements > 0
                ? Math.Round(totalProgress / totalRequirements, 2)
                : 0;
            projectProgressDict[projectId] = overallProgress;
        }
    }
}
