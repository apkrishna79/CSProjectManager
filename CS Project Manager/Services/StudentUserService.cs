/*
* Prologue
Created By: Isabel Loney
Date Created: 2/25/25
Last Revised By: Anakha Krishna
Date Revised: 3/2/25
Purpose: Provides data access methods for user-related operations in the MongoDB database

Preconditions: MongoDB setup, StudentUsers table exists, StudentUser model defined
Postconditions: StudentUser retrieved by username, new users can be inserted
Error and exceptions: MongoDB.Driver.MongoException (thrown if there is an issue with the MongoDB connection or operations), ArgumentNullException (thrown if the email, username, or user parameter is null)
Side effects: N/A
Invariants: _studentUsers collection is always initialized with the "StudentUsers" collection from the MongoDB database
Other faults: N/A
*/


using MongoDB.Driver;  
using CS_Project_Manager.Models;
using MongoDB.Bson;

namespace CS_Project_Manager.Services
{
    public class StudentUserService(MongoDBService mongoDBService)
    {
        // MongoDB collection that stores StudentUser documents
        private readonly IMongoCollection<StudentUser> _studentUsers = mongoDBService.GetCollection<StudentUser>("StudentUsers");

        // Retrieves a User document based on the provided username; returns null if not found
        public async Task<StudentUser> GetUserByUsernameAsync(string username)
        {
            return await _studentUsers.Find(user => user.Username == username).FirstOrDefaultAsync();
        }

        // Inserts a new User document into the collection asynchronously
        public async Task CreateUserAsync(StudentUser user) =>
            await _studentUsers.InsertOneAsync(user);

        public async Task UpdateUserEmailAsync(ObjectId studentId, string newEmail) =>
            await _studentUsers.UpdateOneAsync(
                u => u.Id == studentId,
                Builders<StudentUser>.Update.Set(u => u.Email, newEmail)
            );
    }
}
