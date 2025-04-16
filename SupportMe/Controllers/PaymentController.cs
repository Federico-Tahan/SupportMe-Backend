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
            var response = await _paymentService.Pay(paymentInformation, id);
            return Ok(response);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Payments()
        {
            UserMiddelware user = (UserMiddelware)HttpContext.Items["UserMiddelware"];
            var response = await _paymentService.GetPayments(user.User.Id);
            return Ok(response);
        }
    }
}
