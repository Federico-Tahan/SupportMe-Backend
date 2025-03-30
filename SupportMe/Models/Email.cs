using System.Net.Mail;

namespace SupportMe.Models
{
    public class Email
    {
        public string FromName { get; set; }

        public string FromEmail { get; set; }

        public List<string> ToAddresses { get; set; }

        /// <summary>
        /// Hidden email addresses
        /// </summary>
        public List<string> ToBccAddresses { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        public List<Attachment> Attachments { get; set; }

        public Email(string subject, string message, string emailToSend)
        {
            this.FromName = "SupportMe";
            this.Subject = subject;
            this.Message = message;
            this.ToAddresses = new List<string>() { emailToSend };
            this.ToBccAddresses = new List<string>();
            this.Attachments = new List<Attachment>();
        }

    }
}
