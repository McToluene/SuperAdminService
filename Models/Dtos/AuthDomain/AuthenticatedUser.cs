namespace SuperAdmin.Service.Models.Dtos.AuthDomain
{
    public class AuthenticatedUser
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string[] Roles { get; set; }
    }
}
