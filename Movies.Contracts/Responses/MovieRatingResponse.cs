using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Contracts.Responses
{
    public class MovieRatingResponse
    {
        public required Guid MovieId { get; set; }
        public required string Slug { get; set; }
        public required int Rating { get; set; }
    }
}
