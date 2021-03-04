using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DopplerFiles.DopplerSecurity
{
    public class IsOwnResourceAuthorizationHandler : AuthorizationHandler<DopplerAuthorizationRequirement>
    {
        private readonly ILogger<IsOwnResourceAuthorizationHandler> _logger;

        public IsOwnResourceAuthorizationHandler(ILogger<IsOwnResourceAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DopplerAuthorizationRequirement requirement)
        {
            if (requirement.AllowOwnResource && IsOwnResource(context))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private bool IsOwnResource(AuthorizationHandlerContext context)
        {
            var tokenUserId = context.User.FindFirst(c => c.Type == DopplerSecurityDefaults.USER_JWT_KEY)?.Value;

            if (!(context.Resource is HttpContext resource) || string.IsNullOrWhiteSpace(tokenUserId))
            {
                _logger.LogWarning("Is not possible access to Resource information.");
                return false;
            }

            if (!(resource.Request.Path.Value ?? string.Empty).Contains(tokenUserId))
            {
                _logger.LogWarning("The IdUser into the token is different that in the route. The user hasn't permissions.");
                return false;
            }

            return true;
        }
    }
}
