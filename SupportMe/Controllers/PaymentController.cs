using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportMe.DTOs.PaymentDTOs;
using SupportMe.MiddleWares;
using SupportMe.Services;

namespace SupportMe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("{id}/campaign")]
        public async Task<IActionResult> Payment([FromBody] PaymentInformation paymentInformation, [FromRoute] int id)
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];
            string? userId = user != null && user.User != null ? user.User.Id : null;

            var response = await _paymentService.Pay(paymentInformation, id, userId);
            return Ok(response);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Payments([FromQuery] PaymentFilter filter)
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];
            var response = await _paymentService.GetPayments(user.User.Id, filter);
            return Ok(response);
        }

        [HttpGet("{chargeId}/donation")]
        public async Task<IActionResult> GetSimplePaymentByChargeId([FromRoute] string chargeId)
        {
            var response = await _paymentService.GetPayments(chargeId);
            return Ok(response);
        }
        [HttpGet("{chargeId}/detail")]
        public async Task<IActionResult> GetDetailPayment([FromRoute] string chargeId)
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];
            var response = await _paymentService.GetPaymentDetail(chargeId, user.User.Id);
            return Ok(response);
        }

        [HttpGet("donations")]
        public async Task<IActionResult> GetDonations([FromQuery] int skip = 0, [FromQuery] int take = 5)
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];
            var response = await _paymentService.GetDonationsByUser(user.User.Id, skip, take);
            return Ok(response);
        }
    }
}
