namespace SupportMe.DTOs.DonationDTOs
{
    public class SimpleDonation
    {
        public string DonatorName { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? CampaignName { get; set; }
        public string? Comment { get; set; }
    }
}
