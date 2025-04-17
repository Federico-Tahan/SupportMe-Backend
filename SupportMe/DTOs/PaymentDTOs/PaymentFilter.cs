using SupportMe.Models;

namespace SupportMe.DTOs.PaymentDTOs
{
    public class PaymentFilter : BaseFilter
    {
        public List<string>? Brand { get; set; }
        public List<int>? CampaignId { get; set; }
        public List<Status>? Status { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
