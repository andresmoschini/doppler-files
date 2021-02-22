using Microsoft.AspNetCore.Mvc;

namespace DopplerFiles.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Test controller";
        }
    }
}
