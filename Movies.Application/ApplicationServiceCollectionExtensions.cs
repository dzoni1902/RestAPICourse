using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Movies.Application.Database;
using Movies.Application.Repositories;
using Movies.Application.Services;

namespace Movies.Application

{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register application services here
            services.AddSingleton<IRatingRepository, RatingRepository>();
            services.AddSingleton<IRatingService, RatingService>(); 
            services.AddSingleton<IMovieRepository, MovieRepository>();
            services.AddSingleton<IMovieService, MovieService>();    //no shared state in the MovieService, doesnt have to be anything else
            services.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Singleton);
            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
        {
            // Singleton masks Transient (factory returns new connection every time)
            services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));
            services.AddSingleton<DbInitializer>();
            return services;
        }
    }
}
