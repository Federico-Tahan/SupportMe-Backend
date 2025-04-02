namespace SupportMe.DTOs.CampaignDTOs
{
    public class CampaignWriteDTO
    {
        public string Name { get; set; }
        public string MainImage { get; set; }
        public string? Description { get; set; }
        public decimal? GoalAmount { get; set; }
        public DateTime? GoalDate { get; set; }
    }
}
