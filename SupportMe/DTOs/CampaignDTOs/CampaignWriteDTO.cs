namespace SupportMe.DTOs.CampaignDTOs
{
    public class CampaignWriteDTO
    {
        public string Name { get; set; }
        public string MainImage { get; set; }
        public string? Description { get; set; }
        public decimal? GoalAmount { get; set; }
        public int CategoryId { get; set; }
        public DateTime? GoalDate { get; set; }
        public List<CampaignAssetsWriteDTO> Assets { get; set; }
        public List<CampaignTagsWriteDTO> Tags { get; set; }

    }

    public class CampaignAssetsWriteDTO 
    {
        public string Base64 { get; set; }
    }
    public class CampaignTagsWriteDTO
    {
        public string Tag { get; set; }
    }
}
