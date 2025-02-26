using MongoDB.Driver;  
using CS_Project_Manager.Models;
using MongoDB.Bson;

namespace CS_Project_Manager.Services
{
    public class TeamService(MongoDBService mongoDBService)
    {
        private readonly IMongoCollection<Team> _teams = mongoDBService.GetCollection<Team>("Teams");

        // Inserts a new Team document into the collection asynchronously
        public async Task CreateTeamAsync(Team team) =>
            await _teams.InsertOneAsync(team);

        public async Task<Team> GetTeamByNameAndClassId(string teamName, ObjectId classId)
        {
            return await _teams.Find(t => t.Name == teamName && t.AssociatedClass == classId).FirstOrDefaultAsync();
        }

        public async Task<List<Team>> GetTeamsByClassNames(Dictionary<ObjectId, string> classIdToName)
        {
            return await _teams
                .Find(t => classIdToName.Keys.Contains(t.AssociatedClass))
                .ToListAsync();
        }

        public async Task AddStudentToTeamAsync(ObjectId teamId, ObjectId studentId)
        {
            var update = Builders<Team>.Update.AddToSet(t => t.Members, studentId);
            await _teams.UpdateOneAsync(t => t.Id == teamId, update);
        }
    }
}
