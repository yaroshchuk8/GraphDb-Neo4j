using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace API.Controllers;

public class MoviesController(IDriver driver) : ApiBaseController
{
    [HttpGet]
    public async Task<List<Movie>> GetAllMovies()
    {
        var movies = new List<Movie>();

        var query = "MATCH (m:Movie) RETURN m.id AS id, m.title AS title";

        await using var session = driver.AsyncSession();

        var cursor = await session.RunAsync(query);

        await cursor.ForEachAsync(record =>
        {
            movies.Add(new Movie
            {
                Id = record["id"].As<string>(),
                Title = record["title"].As<string>()
            });
        });

        return movies;
    }
    
    [HttpGet("{movieId}")]
    public async Task<Movie> GetMovieById(string movieId)
    {
        var query = "MATCH (m:Movie {id: $id}) RETURN m";
        var session = driver.AsyncSession();
        try
        {
            var result = await session.RunAsync(query, new { id = movieId });
            var record = await result.SingleAsync();
            var node = record["m"].As<INode>();

            return new Movie
            {
                Id = node.Properties["id"].As<string>(),
                Title = node.Properties["title"].As<string>()
            };
        }
        finally
        {
            await session.CloseAsync();
        }
    }
    
    [HttpPost]
    public async Task<string> CreateMovie(string title)
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid().ToString(),
            Title = title
        };
        
        var query = "CREATE (m:Movie {id: $id, title: $title})";
        var session = driver.AsyncSession();
        try
        {
            await session.RunAsync(query, new { id = movie.Id, title = movie.Title });
        }
        finally
        {
            await session.CloseAsync();
        }

        return movie.Id;
    }
    
    [HttpGet("average-ratings")]
    public async Task<List<MovieWithAverageRating>> GetMoviesWithAverageRatings()
    {
        var movies = new List<MovieWithAverageRating>();

        var query = @"
            MATCH (m:Movie)
            OPTIONAL MATCH (m)<-[:`For movie`]-(r:Review)
            WITH m, avg(r.rating) AS avgRating
            RETURN m.id AS id, m.title AS title,
                   CASE WHEN avgRating IS NULL THEN 0 ELSE avgRating END AS averageRating
        ";

        await using var session = driver.AsyncSession();

        var cursor = await session.RunAsync(query);

        await cursor.ForEachAsync(record =>
        {
            movies.Add(new MovieWithAverageRating
            {
                Id = record["id"].As<string>(),
                Title = record["title"].As<string>(),
                AverageRating = record["averageRating"].As<double>()
            });
        });

        return movies;
    }
    
    // Delete all users
    [HttpDelete]
    public async Task DeleteAllMovies()
    {
        var query = "MATCH (m:Movie) DETACH DELETE m";

        await using var session = driver.AsyncSession();
        await session.RunAsync(query);
    }
}