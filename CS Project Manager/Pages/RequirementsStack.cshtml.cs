/*
 * Prologue
Created By: Dylan Sailors
Date Created: 3/1/25
Last Revised By: Dylan Sailors
Date Revised: 3/30/25 GK - mark requirements as complete
Purpose: Let users add/update/remove/export/mark requirements to a blank requirements stack generated once a project is created
Preconditions: MongoDBService, ProjectService instances properly initialized and injected; Requirement must be correctly defined
Postconditions: Users can add, update, and remove project requirements
Error and exceptions: ArgumentNullException (required field empty), FormatException (invalid data input)
Side effects: N/A
Invariants: Requirements list is always initialized; OnGet method always prepares an initial list
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using ClosedXML.Excel; //library that exports files as an excel file
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CS_Project_Manager.Pages
{
    public class RequirementsStackModel : PageModel
    {
        private readonly RequirementService _requirementService;
        private readonly ProjectService _projectService;
        private readonly TeamService _teamService;
        private readonly StudentUserService _studentUserService;

        [BindProperty]
        public List<Requirement> Requirements { get; set; } = new List<Requirement>();

        [BindProperty]
        public Requirement NewRequirement { get; set; } = new Requirement
        {
            Description = string.Empty
        };

        [BindProperty(SupportsGet = true)]
        public ObjectId ProjectId { get; set; }
        [BindProperty(SupportsGet = true)]
        public ObjectId AssocTeamId { get; set; }
        public List<StudentUser> TeamMembers = new List<StudentUser>();      
        public RequirementsStackModel(RequirementService requirementService, ProjectService projectService, TeamService teamService, StudentUserService studentUserService)
        {
            _requirementService = requirementService;
            _projectService = projectService;
            _teamService = teamService;
            _studentUserService = studentUserService;
        }

        public async Task OnGetAsync(string projectId)
        {
            if (!ObjectId.TryParse(projectId, out ObjectId parsedProjectId))
            {
                throw new ArgumentException("Invalid project ID format");
            }
            ProjectId = parsedProjectId;
            Requirements = await _requirementService.GetRequirementsByProjectIdAsync(parsedProjectId);

            Requirements = Requirements
                .OrderBy(r => r.RequirementID ?? int.MaxValue)
                .ToList();

            var currentProject = await _projectService.GetProjectById(ProjectId);
            var curTeam = await _teamService.GetTeamByIdAsync(currentProject.AssociatedTeam);
            AssocTeamId = currentProject.AssociatedTeam;

            var users = await Task.WhenAll(curTeam.Members
                .Select(member => _studentUserService.GetUserByIdAsync(member)));

            TeamMembers = users.Where(user => user != null).ToList();
        }

        // Add a new requirement
        public async Task<IActionResult> OnPostAddAsync(ObjectId projectId)
        {
            if (!NewRequirement.RequirementID.HasValue)
            {
                ModelState.AddModelError("NewRequirement.RequirementID", "Requirement ID cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(NewRequirement.Description))
            {
                ModelState.AddModelError("NewRequirement.Description", "Description cannot be empty.");
            }

            // Fetch existing requirements for the project
            var existingRequirements = await _requirementService.GetRequirementsByProjectIdAsync(projectId);

            // Check for duplicate RequirementID
            if (existingRequirements.Any(r => r.RequirementID == NewRequirement.RequirementID))
            {
                ModelState.AddModelError("NewRequirement.RequirementID", "A requirement with this Requirement ID already exists.");
            }

            if (!ModelState.IsValid)
            {
                ProjectId = projectId;
                Requirements = existingRequirements
                    .OrderBy(r => r.RequirementID ?? int.MaxValue)
                    .ToList();
                return Page();
            }

            ProjectId = projectId;
            var project = await _projectService.GetProjectById(ProjectId);
            if (project == null)
            {
                return NotFound();
            }

            NewRequirement.AssocProjectId = ProjectId;
            await _requirementService.AddRequirementAsync(NewRequirement);

            Requirements = (await _requirementService.GetRequirementsByProjectIdAsync(projectId))
                .OrderBy(r => r.RequirementID ?? int.MaxValue)
                .ToList();

            return RedirectToPage(new { projectId = ProjectId.ToString() });
        }


        // Update a requirement
        public async Task<IActionResult> OnPostUpdateAsync(ObjectId id, ObjectId projectId)
        {
            var existingRequirement = await _requirementService.GetRequirementByIdAsync(id);
            if (existingRequirement == null)
            {
                return NotFound();
            }
            var updatedRequirement = Requirements.FirstOrDefault(r => r.Id == id);
            if (updatedRequirement != null)
            {
                existingRequirement.RequirementID = updatedRequirement.RequirementID;
                existingRequirement.Description = updatedRequirement.Description;
                existingRequirement.StoryPoints = updatedRequirement.StoryPoints;
                existingRequirement.Priority = updatedRequirement.Priority;
                existingRequirement.SprintNo = updatedRequirement.SprintNo;
                existingRequirement.Assignees = updatedRequirement.Assignees?.ToList() ?? new List<ObjectId>();

                await _requirementService.UpdateRequirementAsync(existingRequirement);
            }
            Requirements = (await _requirementService.GetRequirementsByProjectIdAsync(projectId))
                .OrderBy(r => r.RequirementID ?? int.MaxValue)
                .ToList();
            return RedirectToPage(new { projectId = projectId.ToString() });
        }
        
        // Remove a requirement
        public async Task<IActionResult> OnPostRemoveAsync(ObjectId id, ObjectId projectId)
        {
            var requirement = await _requirementService.GetRequirementByIdAsync(id);
            if (requirement == null)
            {
                return NotFound();
            }
            await _requirementService.RemoveRequirementAsync(id);
            Requirements = (await _requirementService.GetRequirementsByProjectIdAsync(projectId))
                .OrderBy(r => r.RequirementID ?? int.MaxValue)
                .ToList();
            return RedirectToPage(new { projectId = projectId.ToString() });
        }

        // Export requirements to Excel
        public async Task<IActionResult> OnPostExportAsync(ObjectId projectId)
        {
            var requirements = await _requirementService.GetRequirementsByProjectIdAsync(projectId);

            if (requirements == null || !requirements.Any())
            {
                Console.WriteLine("No requirements found for this project.");
            }
            requirements = requirements
                .OrderBy(r => r.RequirementID ?? int.MaxValue)
                .ToList();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Requirements Stack");
                worksheet.Cell(1, 1).Value = "Requirement ID";
                worksheet.Cell(1, 2).Value = "Description";
                worksheet.Cell(1, 3).Value = "Story Points";
                worksheet.Cell(1, 4).Value = "Priority";
                worksheet.Cell(1, 5).Value = "Sprint Number";
                worksheet.Cell(1, 6).Value = "Assignees";
                int row = 2;
                foreach (var req in requirements)
                {
                    worksheet.Cell(row, 1).Value = req.RequirementID?.ToString() ?? "N/A";
                    worksheet.Cell(row, 2).Value = string.IsNullOrEmpty(req.Description) ? "No Description" : req.Description;
                    worksheet.Cell(row, 3).Value = req.StoryPoints?.ToString() ?? "No Story Points";
                    worksheet.Cell(row, 4).Value = req.Priority?.ToString() ?? "No Priority";
                    worksheet.Cell(row, 5).Value = req.SprintNo?.ToString() ?? "No Sprint No";
                    // Fetch names for assignees
                    if (req.Assignees != null && req.Assignees.Any())
                    {
                        var assigneeNames = new List<string>();
                        foreach (var assigneeId in req.Assignees)
                        {
                            var assignee = await _studentUserService.GetUserByIdAsync(assigneeId);
                            if (assignee != null)
                            {
                                assigneeNames.Add($"{assignee.FirstName} {assignee.LastName}");
                            }
                        }
                        worksheet.Cell(row, 6).Value = string.Join(", ", assigneeNames);
                    }
                    else
                    {
                        worksheet.Cell(row, 6).Value = "No Assignees";
                    }

                    row++;
                }
                // Auto-fit columns for better formatting
                worksheet.Columns().AdjustToContents();
                // Use ClosedXML to create the Excel file
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RequirementsStack.xlsx");
                }
            }
        }

        //toggle requirements as complete
        public async Task<IActionResult> OnPostToggleCompleteAsync(string id, string projectId)
        {
            if (!ObjectId.TryParse(id, out ObjectId requirementId))
            {
                return BadRequest("Invalid requirement ID");
            }

            if (!ObjectId.TryParse(projectId, out ObjectId projectObjectId))
            {
                return BadRequest("Invalid project ID");
            }

            var requirement = await _requirementService.GetRequirementByIdAsync(requirementId);
            if (requirement != null)
            {
                requirement.IsComplete = !requirement.IsComplete;
                await _requirementService.UpdateRequirementAsync(requirement);
            }

            return RedirectToPage(new { projectId = projectId });
        }
    }
}
