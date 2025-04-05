namespace SupportMe.DTOs.UserDTOs
{
    public class LoginDTO
    {
        public string Token { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePic { get; set; }
        public string Email { get; set; }
    }
}
