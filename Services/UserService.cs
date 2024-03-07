using BookStoreApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BookStoreApi.Services;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(IOptions<UserDatabaseSettings> userDatabaseSettings)
    {
        var mongoClient = new MongoClient(userDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(userDatabaseSettings.Value.DatabaseName);
        _users = mongoDatabase.GetCollection<User>(userDatabaseSettings.Value.UserCollectionName);
    }

    public async Task<List<User>> GetAsync() => await _users.Find(user => true).ToListAsync();

    public async Task<User?> GetAsync(string id) => await _users.Find(user => user.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(User newUser) => await _users.InsertOneAsync(newUser);
}