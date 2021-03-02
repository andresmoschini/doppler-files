using DopplerFiles.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DopplerFiles.Controllers
{
    [ApiController]
    [Authorize(nameof(IsDopplerFilesUserRequirement))]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string AuthorizedAccess()
        {
            return "Authorized !!!";
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("anonymous")]
        public string AnonymousAccess()
        {
            return "Anonymous !!!";
        }
    }
}
