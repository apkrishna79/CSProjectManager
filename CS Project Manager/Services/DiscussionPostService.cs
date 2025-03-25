using CS_Project_Manager.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CS_Project_Manager.Services
{
    public class DiscussionPostService(MongoDBService mongoDBService)
    {
        private readonly IMongoCollection<DiscussionPost> _discussionPosts = mongoDBService.GetCollection<DiscussionPost>("DiscussionPosts");

        public async Task CreateDiscussionPostAsync(DiscussionPost newPost) =>
            await _discussionPosts.InsertOneAsync(newPost);

        public async Task<List<DiscussionPost>> GetDiscussionPostsByBoardId(ObjectId boardId)
        {
            var filter = Builders<DiscussionPost>.Filter.Eq(p => p.BoardId, boardId);
            return await _discussionPosts.Find(filter).ToListAsync();
        }

        public async Task<DiscussionPost> GetDiscussionPostByIdAsync(ObjectId postId)
        {
            return await _discussionPosts.Find(p => p.Id == postId).FirstOrDefaultAsync();
        }

        public async Task UpdateDiscussionPostAsync(DiscussionPost updatedPost)
        {
            var filter = Builders<DiscussionPost>.Filter.Eq(p => p.Id, updatedPost.Id);
            await _discussionPosts.ReplaceOneAsync(filter, updatedPost);
        }

        // delete specified post AND all replies
        public async Task DeleteDiscussionPost(ObjectId postId)
        {
            var filter = Builders<DiscussionPost>.Filter.Or(
                Builders<DiscussionPost>.Filter.Eq(p => p.Id, postId),
                Builders<DiscussionPost>.Filter.Eq(p => p.HeadPostId, postId)
            );

            await _discussionPosts.DeleteManyAsync(filter);
        }

        public async Task<List<DiscussionPost>> GetRepliesRecursivelyAsync(List<ObjectId> replyIds)
        {
            if (replyIds == null || !replyIds.Any()) return [];

            // Fetch all replies at once instead of making multiple calls
            var replies = await _discussionPosts
                .Find(post => replyIds.Contains(post.Id))
                .SortBy(post => post.Timestamp) // Sort by post time
                .ToListAsync();

            // Recursively fetch replies for each reply's ReplyIds
            foreach (var reply in replies)
            {
                var nestedReplies = await GetRepliesRecursivelyAsync(reply.ReplyIds);
                reply.ReplyIds = nestedReplies.Select(r => r.Id).ToList(); // Ensure only Ids are stored
            }

            return replies;
        }

    }
}
