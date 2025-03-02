/*
* Prologue
Created By: Isabel Loney
Date Created: 2/25/25
Last Revised By: Anakha Krishna
Date Revised: 3/2/25
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

        public async Task<Dictionary<ObjectId, string>> GetClassIdToNameById(ObjectId[] classIds)
        {
            var classMapping = await _classes
                .Find(c => classIds.Contains(c.Id))
                .Project(c => new { c.Id, c.Name })
                .ToListAsync();
            return classMapping.ToDictionary(c => c.Id, c => c.Name);
        }

        public async Task<List<Class>> GetClassesForStudentAsync(ObjectId studentId)
        {
            return await _classes.Find(c => c.EnrolledStudents.Contains(studentId)).ToListAsync();
        }

        public async Task RemoveStudentFromClassAsync(ObjectId classId, ObjectId studentId)
        {
            var update = Builders<Class>.Update.Pull(c => c.EnrolledStudents, studentId);
            await _classes.UpdateOneAsync(c => c.Id == classId, update);
        }
    }
}
