using MongoDB.Driver;  
using CS_Project_Manager.Models;

namespace CS_Project_Manager.Services
{
    public class StudentUserService
    {
        // MongoDB collection that stores User documents
        private readonly IMongoCollection<StudentUser> _studentUsers;

        public StudentUserService(MongoDBService mongoDBService)
        {
            _studentUsers = mongoDBService.GetCollection<StudentUser>("StudentUsers"); 
        }

        // Retrieves a User document based on the provided email; returns null if not found
        public async Task<StudentUser> GetUserByEmailAsync(string email) =>
            await _studentUsers.Find(user => user.Email == email).FirstOrDefaultAsync();

        // Retrieves a User document based on the provided username; returns null if not found
        public async Task<StudentUser> GetUserByUsernameAsync(string username)
        {
            return await _studentUsers.Find(user => user.Username == username).FirstOrDefaultAsync();
        }

        // Inserts a new User document into the collection asynchronously
        public async Task CreateUserAsync(StudentUser user) =>
            await _studentUsers.InsertOneAsync(user);
    }
}
