/*
* Prologue
Created By: Jackson Wunderlich
Date Created: 2/25/25
Last Revised By: Dylan Sailors
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

using CS_Project_Manager.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CS_Project_Manager.Services
{
    public class RequirementService
    {
        private readonly IMongoCollection<Requirement> _requirements;

        public RequirementService(MongoDBService mongoDBService)
        {
            _requirements = mongoDBService.GetCollection<Requirement>("Requirements");
        }

        // Adds a new requirement to the collection
        public async Task AddRequirementAsync(Requirement newRequirement) =>
            await _requirements.InsertOneAsync(newRequirement);

        // Gets all requirements for a specific project by project ID
        public async Task<List<Requirement>> GetRequirementsByProjectIdAsync(ObjectId projectId)
        {
            var filter = Builders<Requirement>.Filter.Eq(r => r.AssocProjectId, projectId);
            return await _requirements.Find(filter).ToListAsync();
        }

        // Gets a requirement by its ID
        public async Task<Requirement?> GetRequirementByIdAsync(ObjectId id) =>
            await _requirements.Find(r => r.Id == id).FirstOrDefaultAsync();

        // Updates a requirement
        public async Task UpdateRequirementAsync(Requirement updatedRequirement)
        {
            var filter = Builders<Requirement>.Filter.Eq(r => r.Id, updatedRequirement.Id);
            await _requirements.ReplaceOneAsync(filter, updatedRequirement);
        }

        // Removes a requirement by its ID
        public async Task RemoveRequirementAsync(ObjectId id)
        {
            var filter = Builders<Requirement>.Filter.Eq(r => r.Id, id);
            await _requirements.DeleteOneAsync(filter);
        }
    }
}
