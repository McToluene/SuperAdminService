namespace SuperAdmin.Service.Models.Dtos.AuthDomain
{
    public class ValidatedLoginCredentials
    {
        public string UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsPinSet { get; set; }
    }
}
