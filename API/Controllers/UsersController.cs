using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace API.Controllers;

public class UsersController(IDriver driver) : ApiBaseController
{
    [HttpGet]
    public async Task<List<User>> GetAllUsers()
    {
        var users = new List<User>();

        var query = "MATCH (u:User) RETURN u.id AS id, u.name AS name";

        await using var session = driver.AsyncSession();

        var cursor = await session.RunAsync(query);

        await cursor.ForEachAsync(record =>
        {
            users.Add(new User
            {
                Id = record["id"].As<string>(),
                Name = record["name"].As<string>()
            });
        });

        return users;
    }
    
    [HttpGet("{userId}")]
    public async Task<User> GetUserByIdAsync(string userId)
    {
        var query = "MATCH (u:User {id: $id}) RETURN u";
        var session = driver.AsyncSession();
        try
        {
            var result = await session.RunAsync(query, new { id = userId });
            var record = await result.SingleAsync();
            var node = record["u"].As<INode>();

            return new User
            {
                Id = node.Properties["id"].As<string>(),
                Name = node.Properties["name"].As<string>()
            };
        }
        finally
        {
            await session.CloseAsync();
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser(string name)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = name
        };
        
        var query = "CREATE (u:User {id: $id, name: $name})";
        var session = driver.AsyncSession();
        try
        {
            await session.RunAsync(query, new { id = user.Id, name = user.Name });
        }
        finally
        {
            await session.CloseAsync();
        }

        return Ok(user.Id);
    }

    [HttpPost("friendship")]
    public async Task CreateFriendshipAsync(string userId1, string userId2)
    {
        var query = @"
            MATCH (a:User {id: $id1}), (b:User {id: $id2})
            MERGE (a)-[:`Friends with`]-(b)
        ";
        var session = driver.AsyncSession();
        try
        {
            await session.RunAsync(query, new { id1 = userId1, id2 = userId2 });
        }
        finally
        {
            await session.CloseAsync();
        }
    }
    
    [HttpGet("{userId}/recommendations")]
    public async Task<List<MovieWithAverageRating>> GetMovieRecommendations(string userId)
    {
        var recommended = new List<MovieWithAverageRating>();

        var query = @"
            MATCH (u:User {id: $userId})-[:`Friends with`]-(friend:User)
            MATCH (friend)-[:`Made review`]->(r:Review)-[:`For movie`]->(m:Movie)
            RETURN m.id AS id, m.title AS title, AVG(r.rating) AS avgRating
            ORDER BY avgRating DESC
        ";

        await using var session = driver.AsyncSession();
        var cursor = await session.RunAsync(query, new { userId });

        await cursor.ForEachAsync(record =>
        {
            recommended.Add(new MovieWithAverageRating
            {
                Id = record["id"].As<string>(),
                Title = record["title"].As<string>(),
                AverageRating = Math.Round(record["avgRating"].As<double>(), 2)
            });
        });

        return recommended;
    }
    
    // Delete all users
    [HttpDelete]
    public async Task DeleteAllUsersAsync()
    {
        var query = "MATCH (u:User) DETACH DELETE u";

        await using var session = driver.AsyncSession();
        await session.RunAsync(query);
    }
}