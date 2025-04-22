namespace SupportMe.DTOs.CampaignDTOs
{
    public class CampaignFilter : BaseFilter
    {
        public int? CategoryId { get; set; }
        public bool OnlyActive { get; set; } = true;
    }
}
