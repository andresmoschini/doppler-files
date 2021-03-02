using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace DopplerFiles.Authorization
{
    public class IsDopplerFilesUserHandler : AuthorizationHandler<IsDopplerFilesUserRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            IsDopplerFilesUserRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type.Equals("isDopplerFilesUser")))
            {
                return Task.CompletedTask;
            }

            var isDopplerFilesUser = bool.Parse(context.User.FindFirst(c => c.Type.Equals("isDopplerFilesUser")).Value);
            if (isDopplerFilesUser)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
