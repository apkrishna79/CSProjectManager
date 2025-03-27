/*
* Prologue
Created By: Isabel Loney
Date Created: 3/17/25
Last Revised By: Isabel Loney
Date Revised: 3/27/25
Purpose: Provides data access methods for discussion board related operations in the MongoDB database

Preconditions: MongoDB setup, DiscussionBoard table exists, DiscussionBoard model defined
Postconditions: DiscussionBoard retrieved by id, classId, new items can be inserted
Error and exceptions: MongoDB.Driver.MongoException (thrown if there is an issue with the MongoDB connection or operations)
Side effects: N/A
Invariants: _discussionBoards collection is always initialized with the "DiscussionBoards" collection from the MongoDB database
Other faults: N/A
*/

using CS_Project_Manager.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CS_Project_Manager.Services
{
    public class DiscussionBoardService(MongoDBService mongoDBService)
    {
        private readonly IMongoCollection<DiscussionBoard> _discussionBoards = mongoDBService.GetCollection<DiscussionBoard>("DiscussionBoards");

        public async Task CreateDiscussionBoardAsync(DiscussionBoard newBoard) =>
            await _discussionBoards.InsertOneAsync(newBoard);

        public async Task<DiscussionBoard> GetDiscussionBoardByClassId(ObjectId classId)
        {
            return await _discussionBoards.Find(c => c.Id == classId).FirstOrDefaultAsync();
        }

        public async Task<DiscussionBoard> GetDiscussionBoardById(ObjectId boardId)
        {
            return await _discussionBoards.Find(c => c.Id == boardId).FirstOrDefaultAsync();
        }


        public async Task<List<DiscussionBoard>> GetDiscussionBoardsByClassIdsAsync(List<ObjectId> classIds)
        {
            var filter = Builders<DiscussionBoard>.Filter.In(d => d.ClassId, classIds);
            return await _discussionBoards.Find(filter).ToListAsync();
        }

        public async Task<List<DiscussionBoard>> GetDiscussionBoardsByTeamIdsAsync(List<ObjectId> teams)
        {
            var filter = Builders<DiscussionBoard>.Filter.In(d => d.TeamId, teams);
            return await _discussionBoards.Find(filter).ToListAsync();
        }
    }
}
