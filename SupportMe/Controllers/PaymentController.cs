using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportMe.DTOs.PaymentDTOs;
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
    }
}
