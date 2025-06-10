using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Movies.Application.Database;

namespace Movies.Application.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public RatingRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition(
                    @"SELECT ROUND(AVG(r.Rating), 1) 
                      FROM Ratings r
                      WHERE MovieId = @MovieId;",
                    new { MovieId = movieId },
                    cancellationToken: token));
        }

        public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QuerySingleOrDefaultAsync<(float? Rating, int? UserRating)>(new CommandDefinition(
                @"SELECT 
                    ROUND(AVG(rating), 1) AS AverageRating,
                    (SELECT TOP 1 rating 
                     FROM Ratings 
                     WHERE movieid = @movieId AND userid = @userId) AS UserRating
                FROM Ratings
                WHERE movieid = @movieId",
                new { MovieId = movieId, UserId = userId },
                cancellationToken: token));
        }
    }
}
