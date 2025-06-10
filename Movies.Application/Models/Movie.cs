using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Movies.Application.Models
{
    public partial class Movie
    {
        public required Guid Id { get; init; }
        public required string Title { get; set; }
        public string Slug => GenerateSlug();
        public float? Rating { get; set; }
        public int? UserRating { get; set; }
        public required int YearOfRelease { get; set; }
        public required List<string> Genres { get; init; } = new();

        private string GenerateSlug()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return string.Empty; 

            var sluggedTitle = SlugRegex().Replace(Title, string.Empty)
                                           .Trim()
                                           .ToLowerInvariant()
                                           .Replace(" ", "-");

            return $"{sluggedTitle}-{YearOfRelease}";
        }


        [GeneratedRegex(@"[^a-zA-Z0-9 _-]", RegexOptions.NonBacktracking, 10)]
        private static partial Regex SlugRegex();
    }
}
