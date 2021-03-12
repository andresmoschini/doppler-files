using Microsoft.AspNetCore.Mvc;

namespace DopplerFiles.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [Route("/error")]
        public IActionResult Error() => Problem();
    }
}
