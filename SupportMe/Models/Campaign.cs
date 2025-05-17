using SupportMe.Models.Interface;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportMe.Models
{
    [Table("Campaign")]
    public class Campaign : IsDeleted
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string MainImage { get; set; }
        public DateTime CreationDate { get; set; }
        public decimal? GoalAmount { get; set; }
        public DateTime? GoalDate { get; set; }
        public string UserId { get; set; }
        public int? CategoryId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public virtual Category? Category { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
        public virtual List<CampaignTags> Tags { get; set; }
        public virtual List<GaleryAssets> Assets { get; set; }

    }
}
