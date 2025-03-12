/*
* Prologue
Created By: Dylan Sailors
Date Created: 3/8/25
Last Revised By: Dylan Sailors
Date Revised: 3/8/25
Purpose: Provides data access methods for requirements-related operations in the MongoDB database
Preconditions: MongoDB setup, Requirements table exists, RequirementsStack model defined
Postconditions: new Requirements can be added, Requirements can be updated and removed
Error and exceptions: MongoDB.Driver.MongoException (thrown if there is an issue with the MongoDB connection or operations)
Side effects: N/A
Invariants: _requirements collection is always initialized with the "Requirements" collection from the MongoDB database
Other faults: N/A
*/

using CS_Project_Manager.Models;
using MongoDB.Driver;
using MongoDB.Bson;
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

        // Adds a new requirement
        public async Task AddRequirementAsync(Requirement newRequirement) =>
            await _requirements.InsertOneAsync(newRequirement);

        // Gets a requirement by its ObjectId
        public async Task<Requirement?> GetRequirementByIdAsync(ObjectId id) =>
            await _requirements.Find(r => r.Id == id).FirstOrDefaultAsync();

        // Gets all requirements associated with a project by project ID
        public async Task<List<Requirement>> GetRequirementsByProjectIdAsync(ObjectId projectId) =>
            await _requirements.Find(r => r.AssocProjectId == projectId).ToListAsync();

        // Updates an existing requirement
        public async Task UpdateRequirementAsync(Requirement updatedRequirement)
        {
            var filter = Builders<Requirement>.Filter.Eq(r => r.Id, updatedRequirement.Id);
            await _requirements.ReplaceOneAsync(filter, updatedRequirement);
        }

        // Removes a requirement by its ObjectId
        public async Task RemoveRequirementAsync(ObjectId id) =>
            await _requirements.DeleteOneAsync(r => r.Id == id);
    }
}
