/*
* Prologue
Created By: Dylan Sailors
Date Created: 3/8/25
Last Revised By: Jackson Wunderlich
Date Revised: 3/16/25
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
using DocumentFormat.OpenXml.Spreadsheet;

namespace CS_Project_Manager.Services
{
    public class RequirementService(MongoDBService mongoDBService)
    {
        private readonly IMongoCollection<Requirement> _requirements = mongoDBService.GetCollection<Requirement>("Requirements");

        // Adds a new requirement
        public async Task AddRequirementAsync(Requirement newRequirement) =>
            await _requirements.InsertOneAsync(newRequirement);

        // Gets a requirement by its ObjectId
        public async Task<Requirement?> GetRequirementByIdAsync(ObjectId id) =>
            await _requirements.Find(r => r.Id == id).FirstOrDefaultAsync();

        // gets requirements associated with a user by their ID
        public async Task<List<Requirement>> GetRequirementsByUserIdAsync(ObjectId id)
        {
            return await _requirements
                .Find(r => r.Assignees.Contains(id))
                .ToListAsync();
        }
        
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

        // Get requirements from multiple projects
        public async Task<List<Requirement>> GetRequirementsByProjectIdsAsync(List<ObjectId> projectIds)
        {
            var filter = Builders<Requirement>.Filter.In(r => r.AssocProjectId, projectIds);
            return await _requirements.Find(filter).ToListAsync();
        }

        // Update Multiple requirements at once
        // Batch update reduces number of DB calls and latency as opposed to using UdpateRequirement in a loop
        public async Task UpdateRequirementsAsync(List<Requirement> requirements)
        {
            var bulkOps = new List<WriteModel<Requirement>>();

            foreach (var req in requirements)
            {
                var filter = Builders<Requirement>.Filter.Eq(r => r.Id, req.Id);
                var update = Builders<Requirement>.Update.Set(r => r.Assignees, req.Assignees);
                bulkOps.Add(new UpdateOneModel<Requirement>(filter, update));
            }

            if (bulkOps.Any())
            {
                await _requirements.BulkWriteAsync(bulkOps);
            }
        }
    }
}
