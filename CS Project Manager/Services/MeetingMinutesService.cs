/*
* Prologue
Created By: Jackson Wunderlich
Date Created: 4/13/25
Last Revised By: Jackson Wunderlich
Date Revised: 4/13/25
Purpose: Provides data access methods for meeting minutes operations in the database
Preconditions: MongoDB setup, MeetingMinutes table exists, MeetingMinutes model defined
Postconditions: new MeetingMinutes items can be added, items can be updated and removed
Error and exceptions: MongoDB.Driver.MongoException (thrown if there is an issue with the MongoDB connection or operations)
Side effects: N/A
Invariants: _minutesItems collection is always initialized with the "MeetingMinutes" collection from the MongoDB database
Other faults: N/A
*/

using CS_Project_Manager.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace CS_Project_Manager.Services
{
    public class MeetingMinutesService
    {
        private readonly IMongoCollection<MeetingMinutes> _minutesItems;

        public MeetingMinutesService(MongoDBService mongoDBService)
        {
            _minutesItems = mongoDBService.GetCollection<MeetingMinutes>("MeetingMinutes");
        }

        // Adds a new meeting minutes item
        public async Task AddMinutesAsync(MeetingMinutes newMinutesItem) =>
            await _minutesItems.InsertOneAsync(newMinutesItem);

        // Gets a minutes item by its ObjectId
        public async Task<MeetingMinutes?> GetMinutesByIdAsync(ObjectId id) =>
            await _minutesItems.Find(m => m.Id == id).FirstOrDefaultAsync();

        // Gets a minutes item by its meeting id
        public async Task<MeetingMinutes?> GetMinutesByMeetingIdAsync(ObjectId meetingId) =>
            await _minutesItems.Find(m => m.AssocMeetingId == meetingId).FirstOrDefaultAsync();

        // Removes a minutes item by its ObjectId
        public async Task RemoveMinutesAsync(ObjectId id) =>
            await _minutesItems.DeleteOneAsync(c => c.Id == id);

        // updates a minutes item
        public async Task UpdateMinutesAsync(MeetingMinutes updatedMinutes)
        {
            var filter = Builders<MeetingMinutes>.Filter.Eq(m => m.Id, updatedMinutes.Id);
            await _minutesItems.ReplaceOneAsync(filter, updatedMinutes);
        }
    }
}
