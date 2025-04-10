using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportMe.Models
{
    [Table("UserMercadoPago")]
    public class UserMercadoPago
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public DateTime CreatedDateUTC { get; set; }
        public int ExpirationSeconds { get; set; }
        public int MPUserId { get; set; }
        public string refresh_token { get; set; }
        public string public_key { get; set; }
        public bool live_mode { get; set; }
    }
}
