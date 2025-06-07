using FluentValidation;
using Movies.Contracts.Responses;

namespace Movies.API.Mapping
{
    public class ValidationMappingMiddleware
    {
        private readonly RequestDelegate _next;
        public ValidationMappingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var validationFailureResponse = new ValidationFailureResponse
                {
                    Errors = ex.Errors.Select(error => new ValidationResponse
                    {
                        PropertyName = error.PropertyName,
                        Message = error.ErrorMessage
                    })
                };

                await context.Response.WriteAsJsonAsync(validationFailureResponse);
            }
        }
    }
}
