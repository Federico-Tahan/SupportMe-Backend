namespace SupportMe.DTOs.CampaignDTOs
{
    public class CampaignReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string MainImage { get; set; }
        public DateTime CreationDate { get; set; }
        public decimal? GoalAmount { get; set; }
        public DateTime? GoalDate { get; set; }
        public List<string> Tags { get; set; }
    }
}
