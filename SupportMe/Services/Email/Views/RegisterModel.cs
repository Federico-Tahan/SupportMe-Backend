using SupportMe.Models;

namespace SupportMe.Services.Email.Views
{
    public class RegisterModel
    {
        public User User { get; set; }
        public string AppUrl { get; set; }
        public string SupportEmail { get; set; }
        public string CompanyName { get; set; }
    }
}
