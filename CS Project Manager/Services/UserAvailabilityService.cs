/*
* Prologue
Created By: Jackson Wunderlich
Date Created: 3/24/25
Last Revised By: Anakha Krishna
Date Revised: 3/28/25
Purpose: Provides data access methods for calendar operations in the database
Preconditions: MongoDB setup, CalendarItems table exists, CalendarItem model defined
Postconditions: new CalendarItem items can be added, items can be updated and removed
Error and exceptions: MongoDB.Driver.MongoException (thrown if there is an issue with the MongoDB connection or operations)
Side effects: N/A
Invariants: _calendarItems collection is always initialized with the "CalendarItems" collection from the MongoDB database
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

        public async Task<UserAvailability?> GetUserAvailabilityByIdAsync(ObjectId id) =>
            await _userAvailability.Find(u => u.Id == id).FirstOrDefaultAsync();

        public async Task<List<UserAvailability>> GetUserAvailabilityByUserIdAsync(ObjectId userId) =>
            await _userAvailability.Find(u => u.AssocUserId == userId).ToListAsync();

        public async Task<List<UserAvailability>> GetUserAvailabilityByTeamIdAsync(ObjectId teamId) =>
            await _userAvailability.Find(u => u.AssocTeamId == teamId).ToListAsync();

        public async Task AddUserAvailabilityAsync(UserAvailability availability) =>
            await _userAvailability.InsertOneAsync(availability);
    }
}
