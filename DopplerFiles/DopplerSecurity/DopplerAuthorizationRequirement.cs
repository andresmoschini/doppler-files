using Microsoft.AspNetCore.Authorization;

namespace DopplerFiles.DopplerSecurity
{
    public class DopplerAuthorizationRequirement : IAuthorizationRequirement
    {
        public bool AllowSuperUser { get; set; }
        public bool AllowOwnResource { get; set; }
    }
}
