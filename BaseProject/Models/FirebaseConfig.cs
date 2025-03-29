using System.ComponentModel.DataAnnotations.Schema;

namespace SupportMe.Models
{
    [Table("FirebaseConfig")]
    public class FirebaseConfig
    {
        public string TenantId { get; set; }
        public string Json { get; set; }
    }
}
