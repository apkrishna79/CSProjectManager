/*
* Prologue
Created By: Anakha Krishna
Date Created: 3/15/25
Last Revised By: Jackson Wunderlich
Date Revised: 4/10/25
Purpose: Provides data access methods for brainstorm-related operations in the MongoDB database

Preconditions: MongoDB setup, BrainstormItems table exists, BrainstormItem model defined
Postconditions: BrainstormItem retrieved by id, projectId, new items can be inserted, items can be updated, items can be removed
Error and exceptions: MongoDB.Driver.MongoException (thrown if there is an issue with the MongoDB connection or operations)
Side effects: N/A
Invariants: _brainstormItems collection is always initialized with the "BrainstormItems" collection from the MongoDB database
Other faults: N/A
*/

using CS_Project_Manager.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CS_Project_Manager.Services
{
    public class BrainstormService
    {
        private readonly IMongoCollection<BrainstormItem> _brainstormItems;

        // MongoDB collection that stores BrainstormItem documents
        public BrainstormService(MongoDBService mongoDBService)
        {
            _brainstormItems = mongoDBService.GetCollection<BrainstormItem>("BrainstormItems");
        }

        // Adds a brainstormItem
        public async Task AddBrainstormItemAsync(BrainstormItem newBrainstormItem) =>
            await _brainstormItems.InsertOneAsync(newBrainstormItem);

        // Retrieves a brainstorm item by its unique objectid
        public async Task<BrainstormItem?> GetBrainstormItemByIdAsync(ObjectId id) =>
            await _brainstormItems.Find(b  => b.Id == id).FirstOrDefaultAsync();

        // Retrieves a list of brainstorm items based on associated projectid attr
        public async Task<List<BrainstormItem>> GetBrainstormItemsByProjectIdAsync(ObjectId projectId) =>
            await _brainstormItems.Find(b => b.AssocProjectId == projectId).ToListAsync();

        // Updates brainstorm item
        public async Task UpdateBrainstormItemAsync(BrainstormItem updatedBrainstormItem)
        {
            var filter = Builders<BrainstormItem>.Filter.Eq(b => b.Id, updatedBrainstormItem.Id);
            await _brainstormItems.ReplaceOneAsync(filter, updatedBrainstormItem);
        }

        // Deletes brainstorm item from database
        public async Task RemoveBrainstormItemAsync(ObjectId id) =>
            await _brainstormItems.DeleteOneAsync(b => b.Id == id);

        // Deletes brainstorm item from database by project id
        public async Task RemoveBrainstormItemsByProjectIdAsync(ObjectId projectId) =>
            await _brainstormItems.DeleteManyAsync(b => b.AssocProjectId == projectId);
    }
}
