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
    }
}
