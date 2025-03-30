using Microsoft.EntityFrameworkCore.Internal;
using System.ComponentModel.DataAnnotations;

namespace SupportMe.Models
{
    public class EmailSenderConfig
    {
        [Key]
        public int ID { get; set; }

        public string FromEmail { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int Port { get; set; }

        public string Host { get; set; }

        public bool EnableSsl { get; set; }
    }
}
