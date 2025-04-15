using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportMe.DTOs;
using SupportMe.DTOs.CampaignDTOs;
using SupportMe.MiddleWares;
using SupportMe.Services;

namespace SupportMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly CampaignService _campaignService;

        public CampaignController(CampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCampaigns([FromQuery] CampaignFilter filter) 
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"]; 
            var campaigns = await _campaignService.GetCampaigns(filter, user?.User?.Id);
            return Ok(campaigns);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCampaignDonation([FromRoute] int id)
        {
            var campaigns = await _campaignService.GetCampaignDonationById(id);
            return Ok(campaigns);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCampaign([FromBody] CampaignWriteDTO request)
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];

            var result = await _campaignService.CreateCampaign(request, user.User.Id);
            return Ok(result);
        }
    }
}
