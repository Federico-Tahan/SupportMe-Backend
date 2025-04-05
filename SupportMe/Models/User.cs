using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportMe.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string? AuthExternalId { get; set; }
        public string? ProfilePic { get; set; }
    }
}
