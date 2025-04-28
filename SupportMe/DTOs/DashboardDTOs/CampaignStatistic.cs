namespace SupportMe.DTOs.DashboardDTOs
{
    public class CampaignStatistic
    {
        public decimal Income { get; set; }
        public decimal? Goal { get; set; }
        public DateTime? GoalDate { get; set; }
        public int Donations { get; set; }
        public int? RemaigninDays { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
