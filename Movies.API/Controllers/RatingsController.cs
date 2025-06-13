using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Auth;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.API.Mapping;
using Asp.Versioning;
using Movies.Contracts.Responses;

namespace Movies.API.Controllers
{
    [ApiController]
    [ApiVersion(1.0)]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [Authorize]
        [HttpPut(ApiEndpoints.Movies.Rate)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RateMovie([FromRoute] Guid movieId, [FromBody] RateMovieRequest request, CancellationToken token)
        {
            var userId = HttpContext.GetUserId();
            var result = await _ratingService.RateMovieAsync(movieId, request.Rating, userId!.Value, token);

            return result ? Ok() : NotFound();
        }


        [Authorize]
        [HttpDelete(ApiEndpoints.Movies.DeleteRating)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRating([FromRoute] Guid movieId, CancellationToken token)
        {
            var userId = HttpContext.GetUserId();

            var result = await _ratingService.DeleteRatingAsync(movieId, userId!.Value, token);
            return result ? Ok() : NotFound();
        }

        [Authorize]
        [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
        [ProducesResponseType(typeof(IEnumerable<MovieRatingResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserRatings(CancellationToken token)
        {
            var userId = HttpContext.GetUserId();
            
            var ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, token);

            var ratingResponse = ratings.MapToResponse();

            return Ok(ratingResponse);
        }
    }
}
