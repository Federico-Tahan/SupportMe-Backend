using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportMe.DTOs;
using SupportMe.DTOs.CampaignDTOs;
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
            var campaigns = await _campaignService.GetCampaigns(filter);
            return Ok(campaigns);
        }


        //[HttpPost]
        //public async Task<IActionResult> CreateCampaign([FromBody] CampaignWriteDTO request) 
        //{
        //    var result = await _campaignService.CreateCampaign(request);
        //    return Ok(result);
        //}
    }
}
