using System.ComponentModel.DataAnnotations.Schema;

namespace SupportMe.Models
{
    [Table("CampaignNotification")]
    public class CampaignNotification
    {
        public int Id { get; set; }
        public DateTime DateUtc { get; set; }
        public string NotificationType { get; set; }
        public int CampaignId { get; set; }
    }
}
