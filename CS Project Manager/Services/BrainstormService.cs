using CS_Project_Manager.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CS_Project_Manager.Services
{
    public class BrainstormService
    {
        private readonly IMongoCollection<BrainstormItem> _brainstormItems;

        public BrainstormService(MongoDBService mongoDBService)
        {
            _brainstormItems = mongoDBService.GetCollection<BrainstormItem>("BrainstormItems");
        }

        public async Task AddBrainstormItemAsync(BrainstormItem newBrainstormItem) =>
            await _brainstormItems.InsertOneAsync(newBrainstormItem);

        public async Task<List<BrainstormItem>> GetBrainstormItemsByProjectIdAsync(ObjectId projectId) =>
            await _brainstormItems.Find(b => b.AssocProjectId == projectId).ToListAsync();

        public async Task UpdateBrainstormItemAsync(BrainstormItem updatedBrainstormItem)
        {
            var filter = Builders<BrainstormItem>.Filter.Eq(b => b.Id, updatedBrainstormItem.Id);
            await _brainstormItems.ReplaceOneAsync(filter, updatedBrainstormItem);
        }

        public async Task RemoveBrainstormItemAsync(ObjectId id) =>
            await _brainstormItems.DeleteOneAsync(b => b.Id == id);
    }
}
