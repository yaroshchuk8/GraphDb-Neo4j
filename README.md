## Requirements: .NET 9 SDK

### How to run application in terminal:
```
git clone https://github.com/yaroshchuk8/GraphDb-Neo4j.git
cd .\GraphDb-Neo4j\API\
dotnet run
```
### Entities
#User { id: string, name: string } <br>
#Movie { id: string, title: string } <br>
#Review { id: string, rating: int } <br>

### DTO:
#MovieWithAverageRating { id: string, title: string, averageRating: double }

## Endpoints
### Users:
<b>GET api/users</b> - returns a list of all #Users <br>
<b>GET api/users/{userId}</b> - returns a #User by id <br>
<b>POST api/users?name=John</b> - creates #User with name John and returns their id <br>
<b>POST api/users/friendship?userId1=guid1&userId2=guid2</b> - creates connection (friendship) between #Users with Ids guid1 and guid2 <br>
<b>GET api/users/{userId}/recommendations</b> - returns list of #MovieWithAverageRating for user by id based on friends' reviews (average rating by friends) <br>

### Movies
<b>GET api/movies</b> - returns a list of all #Movies <br>
<b>GET api/movies/{movieId}</b> - returns a #Movie by id <br>
<b>POST api/movies?title=Titanic</b> - creates #Movie with title Titanic and returns their id <br>
<b>GET api/movies/average-ratings</b> - return a list of #MovieWithAverageRating based on reviews. If no reviews are made, returns rating 0. <br>

### Reviews
<b>POST api/reviews?userId=id1&movieId=id2&rating=7</b> - creates review with rating 7 made by #User with id=id1 for #Movie with id=id2 