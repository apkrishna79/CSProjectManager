/*
* Prologue
Created By: Jackson Wunderlich
Date Created: 2/25/25
Last Revised By: Jackson Wunderlich
Date Revised: 3/2/25
Revision: Added support to add requirements to a project along with updating it and removing it
Purpose: Provides data access methods for project-related operations in the MongoDB database
Preconditions: MongoDB setup, Project table exists, Project model defined
Postconditions: new Projects can be added, Projects can be retrieved by name and ID, a student can be added to a Project
Error and exceptions: MongoDB.Driver.MongoException (thrown if there is an issue with the MongoDB connection or operations), ArgumentNullException (thrown if the email, username, or user parameter is null)
Side effects: N/A
Invariants: _projects collection is always initialized with the "Projects" collection from the MongoDB database
Other faults: N/A
*/
using MongoDB.Driver;
using CS_Project_Manager.Models;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CS_Project_Manager.Services
{
    public class ProjectService(MongoDBService mongoDBService)
    {
        // MongoDB collection that stores Projects
        private readonly IMongoCollection<Project> _projects = mongoDBService.GetCollection<Project>("Projects");

        // adds a new Project to the MongoDB collection
        public async Task CreateProjectAsync(Project newProj) =>
            await _projects.InsertOneAsync(newProj);

        // gets a Project by the provided name, returns null if no project exists
        public async Task<Project?> GetProjectByNameAsync(string name) =>
            await _projects.Find(p => p.project_name == name).FirstOrDefaultAsync();

        // gets a Project by the provided project ID, returns null if no project exists
        public async Task<Project?> GetProjectById(ObjectId projectId) =>
            await _projects.Find(p => p.Id == projectId).FirstOrDefaultAsync();

        // Updates an entire project (including its requirements)
        public async Task UpdateProjectAsync(Project updatedProject)
        {
            var filter = Builders<Project>.Filter.Eq(p => p.Id, updatedProject.Id);
            await _projects.ReplaceOneAsync(filter, updatedProject);
        }

        public async Task<List<Project>> GetProjectsByTeamIdAsync(ObjectId teamId)
        {
            var filter = Builders<Project>.Filter.Eq(p => p.AssociatedTeam, teamId);
            var projects = await _projects.Find(filter).ToListAsync();
            return projects;
        }

        // Adds a new requirement to a project
        public async Task AddRequirementAsync(ObjectId projectId, Requirement newRequirement)
        {
            var project = await GetProjectById(projectId);
            if (project == null) return;

            project.Requirements.Add(newRequirement);
            await UpdateProjectAsync(project);
        }

        // Removes a requirement from a project
        public async Task RemoveRequirementAsync(ObjectId projectId, string requirementId)
        {
            var project = await GetProjectById(projectId);
            if (project == null) return;

            project.Requirements.RemoveAll(r => !string.IsNullOrEmpty(r.RequirementID) && r.RequirementID == requirementId);
            await UpdateProjectAsync(project);
        }
    }
}
