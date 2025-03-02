/*
* Prologue
Created By: Isabel Loney
Date Created: 2/25/25
Last Revised By: Anakha Krishna
Date Revised: 3/2/25
Purpose: Provides data access methods for team-related operations in the MongoDB database

Preconditions: MongoDB setup, Teams table exists, Team model defined
Postconditions: Team retrieved by Id, new teams can be inserted
Error and exceptions: MongoDB.Driver.MongoException (thrown if there is an issue with the MongoDB connection or operations), ArgumentNullException (thrown if the email, username, or user parameter is null)
Side effects: N/A
Invariants: _teams collection is always initialized with the "Teams" collection from the MongoDB database
Other faults: N/A
*/

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

        public async Task<List<Team>> GetTeamsByStudentIdAsync(ObjectId studentId)
        {
            return await _teams
                .Find(t => t.Members.Contains(studentId)) // Check if studentId is in the members array
                .ToListAsync();
        }

        public async Task<List<Team>> GetAllTeams()
        {
            return await _teams.Find(_ => true).ToListAsync();
        }

        public async Task RemoveStudentFromTeamAsync(ObjectId teamId, ObjectId studentId)
        {
            var update = Builders<Team>.Update.Pull(t => t.Members, studentId);
            await _teams.UpdateOneAsync(t => t.Id == teamId, update);
        }

        public async Task<Team> GetTeamByIdAsync(ObjectId teamId)
        {
            return await _teams.Find(t => t.Id == teamId).FirstOrDefaultAsync();
        }
    }
}
