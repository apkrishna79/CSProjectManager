/*
* Prologue
Created By: Jackson Wunderlich
Date Created: 3/24/25
Last Revised By: Jackson Wunderlich
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

namespace CS_Project_Manager.Services
{
    public class CalendarService
    {
        private readonly IMongoCollection<CalendarItem> _calendarItems;
        private readonly IMongoCollection<UserAvailability> _userAvailability;

        public CalendarService(MongoDBService mongoDBService)
        {
            _calendarItems = mongoDBService.GetCollection<CalendarItem>("CalendarItems");
            _userAvailability = mongoDBService.GetCollection<UserAvailability>("UserAvailabilityItems");
        }

        // Adds a new calendar item
        public async Task AddCalendarItemAsync(CalendarItem newCalendarItem) =>
            await _calendarItems.InsertOneAsync(newCalendarItem);

        // Gets a calendar item by its ObjectId
        public async Task<CalendarItem?> GetCalendarItemByIdAsync(ObjectId id) =>
            await _calendarItems.Find(c => c.Id == id).FirstOrDefaultAsync();

        // Gets all calendar items associated with a project by team ID
        public async Task<List<CalendarItem>> GetCalendarItemsByTeamIdAsync(ObjectId teamId) =>
            await _calendarItems.Find(c => c.AssocTeamId == teamId).ToListAsync();

        // Removes a calendar item by its ObjectId
        public async Task RemoveCalendarItemAsync(ObjectId id) =>
            await _calendarItems.DeleteOneAsync(c => c.Id == id);

        // gets a user availability item by its ID
        public async Task GetUserAvailabilityByIdAsync(ObjectId id) =>
            await _userAvailability.Find(u => u.Id == id).FirstOrDefaultAsync();

        // gets user availability items by their team ID
        public async Task<List<UserAvailability>> GetUserAvailabilityByUserIdAsync(ObjectId userId) =>
            await _userAvailability.Find(u => u.AssocUserId == userId).ToListAsync();
    }
}