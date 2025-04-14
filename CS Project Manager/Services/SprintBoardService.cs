/*
* Prologue
Created By: Ginny Ke
Date Created: 4/12/25
Last Revised By: Ginny Ke
Date Revised: 4/13/25
Purpose: Provides data access methods for sprint retrospective board operations in the MongoDB database

Preconditions: MongoDB setup, SprintBoard table exists, SprintBoard model defined
Postconditions: SprintBoard, retrieved by id, projectId, new items can be inserted, items can be updated, items can be removed
Error and exceptions: MongoDB.Driver.MongoException (thrown if there is an issue with the MongoDB connection or operations)
Side effects: N/A
Invariants: _springBoard collection is always initialized with the "SprintBoard" collection from the MongoDB database
Other faults: N/A
*/
using CS_Project_Manager.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CS_Project_Manager.Services
{
    public class SprintBoardService
    {
        private readonly IMongoCollection<SprintBoardItem> _sprintBoardItems;

        public SprintBoardService(MongoDBService mongoDBService)
        {
            _sprintBoardItems = mongoDBService.GetCollection<SprintBoardItem>("SprintBoard");
        }
// handles add item 
        public async Task AddItemAsync(SprintBoardItem item) =>
            await _sprintBoardItems.InsertOneAsync(item);
//gets item by id functionality
        public async Task<SprintBoardItem?> GetItemByIdAsync(ObjectId id) =>
            await _sprintBoardItems.Find(item => item.Id == id).FirstOrDefaultAsync();
//get items by project id functionality
        public async Task<List<SprintBoardItem>> GetItemsByProjectIdAsync(ObjectId projectId) =>
            await _sprintBoardItems.Find(item => item.AssocProjectId == projectId).ToListAsync();
//updates item funcitonality
        public async Task UpdateItemAsync(SprintBoardItem updatedItem)
        {
            var filter = Builders<SprintBoardItem>.Filter.Eq(item => item.Id, updatedItem.Id);
            await _sprintBoardItems.ReplaceOneAsync(filter, updatedItem);
        }
//remove item functionality
        public async Task RemoveItemAsync(ObjectId id) =>
            await _sprintBoardItems.DeleteOneAsync(item => item.Id == id);
    }
}