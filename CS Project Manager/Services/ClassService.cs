/*
* Prologue
Created By: Isabel Loney
Date Created: 2/25/25
Last Revised By: Jackson Wunderlich
Date Revised: 3/1/25
Purpose: Provides data access methods for class-related operations in the MongoDB database

Preconditions: MongoDB setup, Class table exists, Class model defined
Postconditions: Class retrieved by Id, new classes can be inserted
Error and exceptions: MongoDB.Driver.MongoException (thrown if there is an issue with the MongoDB connection or operations), ArgumentNullException (thrown if the email, username, or user parameter is null)
Side effects: N/A
Invariants: _classes collection is always initialized with the "Classes" collection from the MongoDB database
Other faults: N/A
*/

using MongoDB.Driver;  
using CS_Project_Manager.Models;
using MongoDB.Bson;

namespace CS_Project_Manager.Services
{
    public class ClassService(MongoDBService mongoDBService)
    {
        // MongoDB collection that stores Class documents
        private readonly IMongoCollection<Class> _classes = mongoDBService.GetCollection<Class>("Classes");

        // Inserts a new Class document into the collection asynchronously
        public async Task CreateClassAsync(Class newClass) =>
            await _classes.InsertOneAsync(newClass);

        public async Task<Class> GetClassByNameAsync(string name)
        {
            return await _classes.Find(c => c.Name == name).FirstOrDefaultAsync();
        }

        // gets a class by its ObjectId
        public async Task<Class> GetClassByIdAsync(ObjectId id)
        {
            return await _classes.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task AddStudentToClassAsync(ObjectId classId, ObjectId studentId)
        {
            var update = Builders<Class>.Update.AddToSet(c => c.EnrolledStudents, studentId);
            await _classes.UpdateOneAsync(c => c.Id == classId, update);
        }

        public async Task<List<Class>> GetAllClasses()
        {
            return await _classes.Find(_ => true).ToListAsync();
        }

        public async Task<Dictionary<ObjectId, string>> GetClassIdToName(string[] cs)
        {
            var classMapping = await _classes
                .Find(c => cs.Contains(c.Name))
                .Project(c => new { c.Id, c.Name })
                .ToListAsync();
            return classMapping.ToDictionary(c => c.Id, c => c.Name);
        }
    }
}
