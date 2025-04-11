using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportMe.MiddleWares;
using SupportMe.Services;

namespace SupportMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SetupController : ControllerBase
    {
        private readonly SetupService _setupService;

        public SetupController(SetupService setupService)
        {
            _setupService = setupService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSetup() 
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];
            var response = await _setupService.GetSetup(user.User.Id);
            return Ok(response);
        }
    }
}
