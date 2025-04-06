using SupportMe.Models;

namespace SupportMe.MiddleWares
{
    public class UserMiddelware
    {
        public User? User { get; set; }
        public string? JWT { get; set; }
    }
}
