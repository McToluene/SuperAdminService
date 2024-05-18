namespace SuperAdmin.Service.Models.Dtos.AuthDomain
{
    public class SendLoginOTP
    {
        public string UserId { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class SendPinResetOTP : SendLoginOTP { }
}
