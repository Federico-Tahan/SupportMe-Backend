namespace SupportMe.DTOs.UserDTOs
{
    public class SimpleUserInfo
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalDonated { get; set; }
        public int DonationsCount { get; set; }
        public string Email { get; set; }
    }
}
