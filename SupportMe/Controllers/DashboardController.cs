using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportMe.DTOs.DashboardDTOs;
using SupportMe.MiddleWares;
using SupportMe.Services;

namespace SupportMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        [Authorize]
        public async Task<IActionResult> GetSummary([FromQuery] DashboardFilter filter)
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];
            var summary = await _dashboardService.GetSummary(filter, user.User.Id);
            return Ok(summary);
        }

        [HttpGet("campaign")]
        [Authorize]
        public async Task<IActionResult> GetCampaignStatistic([FromQuery] DashboardFilter filter)
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];
            var summary = await _dashboardService.GetCampaignStatistics(filter, user.User.Id);
            return Ok(summary);
        }

        [HttpGet("graph/income")]
        [Authorize]
        public async Task<IActionResult> GetGraphIncome([FromQuery] DashboardFilter filter)
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];
            var summary = await _dashboardService.GetDonationsIncome(filter, user.User.Id);
            return Ok(summary);
        }
        [HttpGet("graph/visit")]
        [Authorize]
        public async Task<IActionResult> GetGraphVisit([FromQuery] DashboardFilter filter)
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];
            var summary = await _dashboardService.GetVisitsGraph(filter, user.User.Id);
            return Ok(summary);
        }
    }
}
