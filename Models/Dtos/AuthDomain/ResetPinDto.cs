namespace SuperAdmin.Service.Models.Dtos.AuthDomain
{
    public class ResetPinDto
    {
        public string UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string NewPin { get; set; }
        public string Otp { get; set; }
    }
}
