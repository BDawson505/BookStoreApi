using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookStoreApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace BookStoreApi.Services;

public class UserService
{
    private readonly IMongoCollection<User> _users;
    private readonly string? key;

    public UserService(IOptions<UserDatabaseSettings> userDatabaseSettings, IConfiguration configuration)
    {
        var mongoClient = new MongoClient(userDatabaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(userDatabaseSettings.Value.DatabaseName);
        _users = mongoDatabase.GetCollection<User>(userDatabaseSettings.Value.UserCollectionName);
        this.key = configuration.GetSection("JwtKey").ToString();
    }

    public async Task<List<User>> GetAsync() => await _users.Find(user => true).ToListAsync();

    public async Task<User?> GetAsync(string id) => await _users.Find(user => user.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(User newUser) => await _users.InsertOneAsync(newUser);

    public async Task<string?> Authenticate(string email, string password)
    {
        var user = await this._users.Find(x => x.Email == email && x.Password == password).FirstOrDefaultAsync();

        if (user is null)
        {
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.ASCII.GetBytes(key);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]{new Claim(ClaimTypes.Email, email),}),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}