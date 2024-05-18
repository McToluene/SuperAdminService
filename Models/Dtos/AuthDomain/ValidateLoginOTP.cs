namespace SuperAdmin.Service.Models.Dtos.AuthDomain
{
    public class ValidateLoginOTP
    {
        public string UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string Otp { get; set; }
    }
}
