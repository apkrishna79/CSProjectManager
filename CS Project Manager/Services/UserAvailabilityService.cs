/*
* Prologue
Created By: Dylan Sailors
Date Created: 3/30/25
Last Revised By: Dylan Sailors
Date Revised: 3/30/25
Purpose: Provides data access methods for user availability operations in the database
Preconditions: MongoDB setup, UserAvailability table exists, UserAvailability model defined
Postconditions: new UserAvailability entries can be added, entries can be updated and removed
Error and exceptions: MongoDB.Driver.MongoException (thrown if there is an issue with the MongoDB connection or operations)
Side effects: N/A
Invariants: _userAvailability collection is always initialized with the "UserAvailability" collection from the MongoDB database
Other faults: N/A
*/

using CS_Project_Manager.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CS_Project_Manager.Services
{
    public class UserAvailabilityService
    {
        private readonly IMongoCollection<UserAvailability> _userAvailability;
        
        public UserAvailabilityService(MongoDBService mongoDBService)
        {
            _userAvailability = mongoDBService.GetCollection<UserAvailability>("UserAvailabilityItems");
        }
        
        // gets the user's availability by their ID
        public async Task<UserAvailability> GetUserAvailabilityByIdAsync(ObjectId id) =>
            await _userAvailability.Find(u => u.Id == id).FirstOrDefaultAsync();
        
        // gets the user's availability by their user ID
        public async Task<List<UserAvailability>> GetUserAvailabilityByUserIdAsync(ObjectId userId) =>
            await _userAvailability.Find(u => u.AssocUserId == userId).ToListAsync();
        
        // gets the user's availability by their team ID
        public async Task<List<UserAvailability>> GetUserAvailabilityByTeamIdAsync(ObjectId teamId) =>
            await _userAvailability.Find(u => u.AssocTeamId == teamId).ToListAsync();
        
        // adds a new user's availability
        public async Task AddUserAvailabilityAsync(UserAvailability availability) =>
            await _userAvailability.InsertOneAsync(availability);
        
        // removes a user's availability by their ID
        public async Task DeleteUserAvailabilityAsync(ObjectId id) =>
            await _userAvailability.DeleteOneAsync(x => x.Id == id);
            
        // updates a user's availability by their ID
        public async Task UpdateUserAvailabilityAsync(UserAvailability availability)
        {
            var filter = Builders<UserAvailability>.Filter.Eq(x => x.Id, availability.Id);
            await _userAvailability.ReplaceOneAsync(filter, availability);
        }
        
        // updates a user's availability team ID by their user ID
        public async Task UpdateUserAvailabilityTeamIdAsync(ObjectId userId, ObjectId newTeamId)
        {
            var filter = Builders<UserAvailability>.Filter.Eq(x => x.AssocUserId, userId);
            var update = Builders<UserAvailability>.Update.Set(x => x.AssocTeamId, newTeamId);
            
            await _userAvailability.UpdateManyAsync(filter, update);
        }
    }
}
