using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportMe.Models
{
    [Table("GaleryAssets")]
    public class GaleryAssets
    {
        [Key]
        public int Id { get; set; }
        public string Asset { get; set; }
        public string AssetSource { get; set; }
        public string AssetSoruceId { get; set; }
    }
}
