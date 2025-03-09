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

using MongoDB.Driver;
using CS_Project_Manager.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace CS_Project_Manager.Services
{
    public class RequirementService
    {
        private readonly IMongoCollection<Requirement> _requirementsCollection;

        public RequirementService(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("CSProMan");
            _requirementsCollection = database.GetCollection<Requirement>("requirements");
        }

        // Create Requirement and associate with Project ID
        public async Task CreateRequirementAsync(Requirement requirement)
        {
            if (requirement.AssocProjectId == ObjectId.Empty)
            {
                throw new ArgumentException("Requirement must be associated with a project.");
            }

            await _requirementsCollection.InsertOneAsync(requirement);
        }


        // Get All Requirements for a Project
        public async Task<List<Requirement>> GetRequirementsByProjectIdAsync(ObjectId projectObjectId)
        {
            var filter = Builders<Requirement>.Filter.Eq(r => r.AssocProjectId, projectObjectId);
            return await _requirementsCollection.Find(filter).ToListAsync();
        }


        // Update Requirement
        public async Task UpdateRequirementAsync(ObjectId id, Requirement updatedRequirement)
        {
            var filter = Builders<Requirement>.Filter.Eq(r => r.Id, id);
            var update = Builders<Requirement>.Update
                .Set(r => r.Description, updatedRequirement.Description)
                .Set(r => r.StoryPoints, updatedRequirement.StoryPoints)
                .Set(r => r.Priority, updatedRequirement.Priority)
                .Set(r => r.SprintNo, updatedRequirement.SprintNo);

            var result = await _requirementsCollection.UpdateOneAsync(filter, update);
            if (result.MatchedCount == 0)
            {
                throw new Exception($"Requirement with ID {id} not found.");
            }
        }


        // Delete Requirement
        public async Task DeleteRequirementAsync(ObjectId id)
        {
            var filter = Builders<Requirement>.Filter.Eq(r => r.Id, id);
            await _requirementsCollection.DeleteOneAsync(filter);
        }
    }
}
