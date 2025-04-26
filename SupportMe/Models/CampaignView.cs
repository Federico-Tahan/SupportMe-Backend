using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportMe.Models
{
    [Table("CampaignView")]
    public class CampaignView
    {
        [Key]
        public int Id { get; set; }
        public DateTime DateUTC { get; set; }
        public int CampaignId { get; set; }
    }
}
