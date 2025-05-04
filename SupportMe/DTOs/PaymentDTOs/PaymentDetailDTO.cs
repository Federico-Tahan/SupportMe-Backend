namespace SupportMe.DTOs.PaymentDTOs
{
    public class PaymentDetailDTO
    {
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string DonatorName { get; set; }
        public string CampaignName { get; set; }
        public string Last4 { get; set; }
        public string Brand { get; set; }
        public DateTime Date { get; set; }
        public string? Comment { get; set; }
        public decimal? CommissionSupportMe { get; set; }
        public decimal? CommissionMP { get; set; }
        public decimal NetReceived { get; set; }

    }
}
