namespace SuperAdmin.Service.Models.Dtos.AuthDomain
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string Token { get; set; }
    }
}
