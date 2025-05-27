using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace API.Controllers;

public class ReviewsController(IDriver driver) : ApiBaseController
{
    [HttpPost]
    public async Task<string> CreateReview(string userId, string movieId, int rating)
    {
        var review = new Review
        {
            Id = Guid.NewGuid().ToString(),
            Rating = rating
        };
        
        var query = @"
            MATCH (u:User {id: $userId}), (m:Movie {id: $movieId})
            CREATE (r:Review {id: $id, rating: $rating})
            MERGE (u)-[:`Made review`]->(r)
            MERGE (r)-[:`For movie`]->(m)
        ";
        
        var session = driver.AsyncSession();
        try
        {
            await session.RunAsync(query, new
            {
                id = review.Id,
                userId = userId,
                movieId = movieId,
                rating = review.Rating
            });
        }
        finally
        {
            await session.CloseAsync();
        }

        return review.Id;
    }
    
    // Delete all users
    [HttpDelete]
    public async Task DeleteAllReviews()
    {
        var query = "MATCH (r:Review) DETACH DELETE r";

        await using var session = driver.AsyncSession();
        await session.RunAsync(query);
    }
}