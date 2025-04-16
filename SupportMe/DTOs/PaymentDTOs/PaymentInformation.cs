using Microsoft.AspNetCore.Http.HttpResults;

namespace SupportMe.DTOs.PaymentDTOs
{
    public class PaymentInformation
    {
        public CardRequest Card { get; set; }
        public int Installments { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string DeviceId { get; set; }
        public string Idempotency { get; set; }
        public string? Description { get; set; }
    }
}
