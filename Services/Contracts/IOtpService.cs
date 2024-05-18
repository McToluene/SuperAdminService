using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface IOtpService
    {
        Task<(bool isSuccessful, string message, string otpCode)> CreateOtpUsingUserPhoneNumber(string userId, string phoneNumber, OtpType otpType);
        Task<(bool isSuccessful, string message, string otpCode)> CreateOtpUsingUserEmail(string userId, string email, OtpType otpType);
        Task<(bool isValid, string message)> ValidateOtpUsingPhoneNumber(string userId, string phoneNumber, string otpCode, OtpType otpType);
    }
}
