using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SupportMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public ActionResult Test() 
        {
            return Ok("Anda");
        }
    }
}
