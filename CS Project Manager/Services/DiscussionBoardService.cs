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

        public async Task<List<DiscussionBoard>> GetDiscussionBoardsByClassIdsAsync(List<ObjectId> classIds)
        {
            var filter = Builders<DiscussionBoard>.Filter.In(d => d.ClassId, classIds);
            return await _discussionBoards.Find(filter).ToListAsync();
        }
    }
}
