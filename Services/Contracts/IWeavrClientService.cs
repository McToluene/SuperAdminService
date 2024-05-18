using Configurations.Utility;
using SuperAdmin.Service.Models.Dtos.WeavrDomain;
using System.Net;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface IWeavrClientService
    {
        Task<ApiResponse<CorporateIdentityResponse>> CreateCorporateIdentity(CorporateIdentityRequest model);

        Task<ApiResponse<string>> InitiateEmailVerificationForCorporates(string email);

        Task<ApiResponse<string>> VerifyEmailForCorporates(VerifyEmail model);

        Task<ApiResponse<PasswordResponse>> CreatePassword(PasswordRequest model);

        Task<ApiResponse<PasswordResponse>> LoginWithPassword(LoginRequest model);

        Task<ApiResponse<KYBResponse>> StartKYB(string token);

        Task<ApiResponse<string>> SendVerificationCodeFor2FA(string token);

        Task<ApiResponse<string>> VerifyCodeFor2FA(string token, StepUpVerification model);

        Task<ApiResponse<KYBStatusResponse>> GetKYBStatus(string token);

        Task<ApiResponse<ManagedAccountResponse>> CreateManagedAccount(CreateManagedAccount model, string token);
    }
}
