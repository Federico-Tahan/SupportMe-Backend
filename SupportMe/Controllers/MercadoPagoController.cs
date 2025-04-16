using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportMe.MiddleWares;
using SupportMe.Services;

namespace SupportMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MercadoPagoController : ControllerBase
    {
        private readonly MercadoPagoService _mp;

        public MercadoPagoController(MercadoPagoService mp)
        {
            _mp = mp;
        }

        [HttpPost("oauth/token/generate")]
        public async Task<IActionResult> OAuthCreateToken([FromQuery] string code) 
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];
            var response = await _mp.ConnectOAuthAccount(code, user.User.Id);
            return Ok(response);
        }
        [HttpDelete("oauth/token")]
        public async Task<IActionResult> DeleteToken()
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];
            var response = await _mp.DeleteToken(user.User.Id);
            return Ok(response);
        }
        [HttpGet("public-key/{id}/campaign")]
        public async Task<IActionResult> GetPublicKey([FromRoute] int id)
        {
            var response = await _mp.GetPublicKeyByCampaignId(id);
            return Ok(response);
        }
    }
}
