using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Contracts.DTOs
{
    public class MovieDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = "";
        public int YearOfRelease { get; init; }
        public float? Rating { get; init; }
        public int? UserRating { get; init; }
        public string Genres { get; init; } = "";
    }
}
