namespace SuperAdmin.Service.Models.Dtos.UserDomain
{
    public class UserDto
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string RoleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public double? DateCreatedInEpochMilliseconds { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
    }
}
