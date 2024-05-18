namespace SuperAdmin.Service.Models.Dtos.WeavrDomain
{
    public class VerifyEmail
    {
        public string email { get; set; }
        public string verificationCode { get; set; }
    }
}
