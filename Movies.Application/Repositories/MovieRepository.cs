using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public MovieRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            using var transaction = connection.BeginTransaction();

            var result = await connection.ExecuteAsync(new CommandDefinition(   //commandDefinition allows us to send also cancellation token
                @"INSERT INTO Movies (Id, Slug, Title, YearOfRelease)
                    VALUES (@Id, @Slug, @Title, @YearOfRelease);",
                movie,
                transaction: transaction,
                cancellationToken: token
            ));

            if (result > 0)
            {
                foreach (var genre in movie.Genres)
                {
                    await connection.ExecuteAsync(new CommandDefinition(
                        @"INSERT INTO Genres (MovieId, Name) VALUES (@MovieId, @Name);",
                        new { MovieId = movie.Id, Name = genre },
                        transaction: transaction,
                        cancellationToken: token
                    ));
                }
            }

            transaction.Commit();
            return result > 0;
        }

        public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
                new CommandDefinition(
                @"SELECT * FROM Movies 
                  WHERE Id = @Id;",
                new { Id = id },
                cancellationToken: token));

            if (movie == null)
            {
                return null;
            }

            var genres = await connection.QueryAsync<string>(
                new CommandDefinition(
                @"SELECT Name FROM Genres 
                  WHERE MovieId = @MovieId;",
                new { MovieId = id }, 
                cancellationToken: token));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }


        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            using var transaction = connection.BeginTransaction();

            // First delete genres associated with the movie
            await connection.ExecuteAsync(new CommandDefinition(
                @"DELETE FROM Genres 
                  WHERE MovieId = @MovieId;",
                new { MovieId = id },
                transaction: transaction,
                cancellationToken: token
            ));

            // Then delete the movie itself
            var result = await connection.ExecuteAsync(new CommandDefinition(
                @"DELETE FROM Movies 
                  WHERE Id = @Id;",
                new { Id = id },
                transaction: transaction,
                cancellationToken: token
            ));

            if (result > 0)
            {
                transaction.Commit();
                return true;
            }
            else
            {
                transaction.Rollback();
                return false;
            }
        }

        public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(
                @"SELECT COUNT(*) > 0 FROM Movies 
                  WHERE Id = @Id;",
                new { Id = id },
                cancellationToken: token));
        }

        public async Task<IEnumerable<Movie>> GetAllAsync(CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.QueryAsync(new CommandDefinition(
                @"SELECT m.*, STRING_AGG(g.name, ',') AS genres
                  FROM movies m 
                  LEFT JOIN genres g ON m.id = g.movieid
                  GROUP BY m.id, m.slug, m.title, m.YearOfRelease",
                cancellationToken: token));

            return result.Select(r => new Movie
            {
                Id = r.Id,
                Title = r.Title,
                YearOfRelease = r.YearOfRelease,
                Genres = Enumerable.ToList(r.genres.Split(','))
            });
        }

        
        public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
                new CommandDefinition(
                @"SELECT * FROM Movies 
                  WHERE Slug = @Slug;",
                new { Slug = slug },
                cancellationToken: token));

            if (movie == null)
            {
                return null;
            }

            var genres = await connection.QueryAsync<string>(
                new CommandDefinition(
                @"SELECT Name FROM Genres 
                  WHERE MovieId = @MovieId;",
                new { MovieId = movie.Id },
                cancellationToken: token));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }

        public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition(
                @"DELETE FROM Genres WHERE MovieId = @MovieId",
                new { MovieId = movie.Id },
                transaction: transaction,
                cancellationToken: token
                ));

            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    @"INSERT INTO Genres (MovieId, Name) VALUES (@MovieId, @Name);",
                    new { MovieId = movie.Id, Name = genre },
                    transaction: transaction,
                    cancellationToken: token
                ));
            }

            var result = await connection.ExecuteAsync(new CommandDefinition(
                @"UPDATE Movies 
                  SET Slug = @Slug, Title = @Title, YearOfRelease = @YearOfRelease 
                  WHERE Id = @Id;",
                movie,
                transaction: transaction,
                cancellationToken: token
            ));

            if (result > 0)
            {
                transaction.Commit();
                return true;
            }
            else
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}
