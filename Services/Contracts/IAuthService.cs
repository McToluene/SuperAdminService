using Configurations.Utility;
using SuperAdmin.Service.Models.Dtos.AuthDomain;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface IAuthService
    {
        Task<ApiResponse<ValidatedLoginCredentials>> ValidateUserLoginCredentials(LoginCredentials model);

        Task<ApiResponse<dynamic>> CreatePinForNewUser(CreatePin model);

        Task<ApiResponse<BasicUserData>> ValidatePin(ValidatePin model);

        Task<ApiResponse<dynamic>> SendLoginOtp(SendLoginOTP model);

        Task<ApiResponse<AuthenticatedUser>> LoginWithValidatedOtp(ValidateLoginOTP model);

        Task<ApiResponse<dynamic>> SendPasswordResetToken(string email);

        Task<ApiResponse<dynamic>> ResetPassword(ResetPasswordDto model);

        Task<ApiResponse<dynamic>> SendPinResetOtp(SendPinResetOTP model);

        Task<ApiResponse<AuthenticatedUser>> ResetPin(ResetPinDto model);
        
        ApiResponse<GeneratedPassword> GeneratePassword();
        
        Task<ApiResponse<ValidatedLoginCredentials>> CreateNewPassword(CreatePassword model);
    }
}
