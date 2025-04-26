using SupportMe.DTOs.CampaignDTOs;
using SupportMe.Models;

namespace SupportMe.DTOs.PaymentDTOs
{
    public class PaymentDetailRead
    {
        public decimal Amount { get; set; }
        public Status Status { get; set; }
        public string Last4 { get; set; }
        public string ChargeId { get; set; }
        public string CustomerName { get; set; }
        public string Brand { get; set; }
        public SimpleCampaignRead Campaign { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}
