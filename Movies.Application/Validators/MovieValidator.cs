using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Services;

namespace Movies.Application.Validators
{
    public class MovieValidator : AbstractValidator<Movie>
    {
        private readonly IMovieRepository _movieRepository;

        public MovieValidator(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;

            RuleFor(movie => movie.Id)
                .NotEmpty();

            RuleFor(movie => movie.Genres)
                .NotEmpty();

            RuleFor(movie => movie.Title)
                .NotEmpty();

            RuleFor(movie => movie.YearOfRelease)
                .LessThanOrEqualTo(DateTime.UtcNow.Year);

            RuleFor(movie => movie.Slug)
                .MustAsync(ValidateSlug)
                .WithMessage("This movie already exists.");
        }

        private async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken token = default)
        {
            var existingMovie = await _movieRepository.GetBySlugAsync(slug);

            if (existingMovie is not null)
            {
                return existingMovie.Id == movie.Id;
            }

            return existingMovie is null;
        }
    }
}
