using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DopplerFiles.Controllers
{
    [ApiController]
    [Authorize]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [Route("[controller]/{UserId?}")]
        public string AuthorizedAccess()
        {
            return "Authorized !!!";
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("[controller]/anonymous")]
        public string AnonymousAccess()
        {
            return "Anonymous !!!";
        }
    }
}
