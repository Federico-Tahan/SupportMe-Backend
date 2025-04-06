using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportMe.DTOs.UserDTOs;
using SupportMe.Services;
using SupportMe.Services.Auth;

namespace SupportMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;

        public UserController(UserService userService, AuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDTO user) 
        {
            var result = await _userService.RegisterUser(user);
            return Ok(result);
        }
        [HttpGet("email")]
        [AllowAnonymous]
        public async Task<IActionResult> Email()
        {
            await _userService.SendEmail();
            return Ok();
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginToken request)
        {
            var response = await _authService.CreateJwtFromFirebaseJwt(request.Token);
            return Ok(response);
        }

        [HttpGet("email/available")]
        [AllowAnonymous]
        public async Task<IActionResult> IsAvailableEmail([FromQuery] string email)
        {
            var response = await _authService.IsAvailableEmail(email);
            return Ok(response);
        }
    }
}
