using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Movies.API.Auth
{
    public class AdminAuthRequirement : IAuthorizationHandler, IAuthorizationRequirement
    {
        private readonly string _apiKey;

        public AdminAuthRequirement(string apiKey)
        {
            _apiKey = apiKey;
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            if (context.User.HasClaim(AuthConstants.AdminUserClaimName, "true"))
            {
                context.Succeed(this);
                return Task.CompletedTask;
            }

            var httpContext = context.Resource as HttpContext;
            if (httpContext is null)
            {
                return Task.CompletedTask;
            }

            if (!httpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Fail(); 
                return Task.CompletedTask;
            }

            if (_apiKey != extractedApiKey)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var identity = (ClaimsIdentity)httpContext.User.Identity!;
            identity.AddClaim(new Claim("userid", Guid.Parse("be1fc7a0-6168-4ab6-b1e9-5af3e0f25411").ToString()));
            context.Succeed(this);
            return Task.CompletedTask;
        }
    }
}
