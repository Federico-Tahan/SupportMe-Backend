using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportMe.Models
{
    [Table("CampaignTags")]
    public class CampaignTags
    {
        [Key]
        public int Id { get; set; }
        public string Tag { get; set; }
        public int CampaignId { get; set; }
    }
}
