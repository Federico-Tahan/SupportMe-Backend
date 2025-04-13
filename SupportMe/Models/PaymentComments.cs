using System.ComponentModel.DataAnnotations.Schema;

namespace SupportMe.Models
{
    [Table("PaymentComments")]
    public class PaymentComments
    {
        public int Id { get; set; }
        public string? Comment { get; set; }
        public int PaymentId { get; set; }
    }
}
