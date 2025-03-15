/*
* Prologue
Created By: Jackson Wunderlich
Date Created: 3/13/25
Last Revised By: Jackson Wunderlich
Date Revised: 3/13/25
Purpose: Provides data access methods for todo-related operations in the MongoDB database
Preconditions: MongoDB setup, Todo table exists, Todo model defined
Postconditions: new Todo items can be added, Todo items can be updated and removed
Error and exceptions: MongoDB.Driver.MongoException (thrown if there is an issue with the MongoDB connection or operations)
Side effects: N/A
Invariants: _todo collection is always initialized with the "Todo" collection from the MongoDB database
Other faults: N/A
*/

using CS_Project_Manager.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CS_Project_Manager.Services
{
    public class TodoService
    {
        private readonly IMongoCollection<Todo> _todo;

        public TodoService(MongoDBService mongoDBService)
        {
            _todo = mongoDBService.GetCollection<Todo>("Todo");
        }

        // Adds a new task
        public async Task AddTodoAsync(Todo newTodo) =>
            await _todo.InsertOneAsync(newTodo);

        // Gets a task by its ObjectId
        public async Task<Todo?> GetTodoByIdAsync(ObjectId id) =>
            await _todo.Find(r => r.Id == id).FirstOrDefaultAsync();

        // Gets all tasks associated with a project by user ID
        public async Task<List<Todo>> GetTodoByUserIdAsync(ObjectId userId) =>
            await _todo.Find(r => r.AssocUserId == userId).ToListAsync();

        // Updates an existing task
        public async Task UpdateTodoAsync(Todo updatedTodo)
        {
            var filter = Builders<Todo>.Filter.Eq(r => r.Id, updatedTodo.Id);
            await _todo.ReplaceOneAsync(filter, updatedTodo);
        }

        // Removes a task by its ObjectId
        public async Task RemoveTodoAsync(ObjectId id) =>
            await _todo.DeleteOneAsync(r => r.Id == id);
    }
}