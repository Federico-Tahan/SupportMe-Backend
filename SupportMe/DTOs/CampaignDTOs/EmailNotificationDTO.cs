namespace SupportMe.DTOs.CampaignDTOs
{
    public class EmailNotificationDTO
    {
        public string CampaignName { get; set; }
        public string OwnerName { get; set; }
        public decimal GoalAmount { get; set; }
        public decimal Income { get; set; }
        public int Donations { get; set; }
    }
}
