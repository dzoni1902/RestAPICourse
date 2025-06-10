using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Movies.Application.Database
{
    public class DbInitializer
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public DbInitializer(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }
        public async Task InitializeAsync()
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            // Perform database initialization logic here, e.g., creating tables, seeding data, etc.

            await connection.ExecuteAsync(@"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Movies' AND xtype='U')
                                            BEGIN
                                                CREATE TABLE Movies (Id UNIQUEIDENTIFIER PRIMARY KEY,       
                                                                     Slug NVARCHAR(255) NOT NULL,      
                                                                     Title NVARCHAR(255) NOT NULL,        
                                                                     YearOfRelease INT NOT NULL);
                                            END");

            await connection.ExecuteAsync(@"IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'movies_slug_idx' 
                                                                                     AND object_id = OBJECT_ID('movies'))
                                            BEGIN
                                                CREATE UNIQUE INDEX movies_slug_idx ON Movies (Slug);
                                            END");

            await connection.ExecuteAsync(@"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Genres')
                                            BEGIN
                                                CREATE TABLE Genres (
                                                    MovieId UNIQUEIDENTIFIER REFERENCES Movies (Id),
                                                    Name NVARCHAR(255) NOT NULL);
                                            END");

            await connection.ExecuteAsync(@"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Ratings' AND xtype='U')
                                            BEGIN
                                                CREATE TABLE Ratings (Userid UNIQUEIDENTIFIER,
                                                                      Movieid UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Movies(id),
                                                                      Rating INT NOT NULL,
                                                                      PRIMARY KEY (Userid, Movieid));
                                            END");
        } 
    }
}
