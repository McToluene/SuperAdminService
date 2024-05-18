using Configurations.Utility;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos.WeavrDomain;
using SuperAdmin.Service.Services.Contracts;
using System.Security.Claims;

namespace SuperAdmin.Service.Services.Implementations
{
    public class WeavrClientService : IWeavrClientService
    {
        private readonly RestHttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _appDbContext;
        public WeavrClientService(RestHttpClient httpClient, IConfiguration config, IHttpContextAccessor httpContextAccessor, AppDbContext context)
        {
            _httpClient = httpClient;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _appDbContext = context;
        }

        /// <summary>
        /// This method creates a new corporate Weavr user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<CorporateIdentityResponse>> CreateCorporateIdentity(CorporateIdentityRequest model)
        {
            var corporateUserExists = await _appDbContext.WeavrCorporateUsers.AnyAsync();
            if (corporateUserExists)
            {
                return new ApiResponse<CorporateIdentityResponse>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Corporate Identity already exists"
                };
            }

            string? ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            if (ip == "::1" || string.IsNullOrWhiteSpace(ip))
            {
                ip = "127.0.0.1";
            }

            var body = new CorporateIdentity
            {
                profileId = _config["WeavrProfiles:Corporates"]!,
                acceptedTerms = true,
                baseCurrency = model.baseCurrency.ToString(),
                company = model.company,
                ipAddress = ip,
                rootUser = model.rootUser,
            };

            var url = _config["WeavrEndpoints:CreateCorporateIdentity"];
            var response = await _httpClient.MakeGenericRequestCall<CorporateIdentityResponse>(Method.Post, url!, body, null);

            if (response.ResponseCode != "200")
            {
                return response;
            }

            string loggedInUserId = _httpContextAccessor.HttpContext!.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            WeavrCorporateUser corporateUser = new WeavrCorporateUser
            {
                BaseCurrency = response.Data!.baseCurrency,
                DayOfBirth = response.Data.rootUser.dateOfBirth.day,
                MonthOfBirth = response.Data.rootUser.dateOfBirth.month,
                YearOfBirth = response.Data.rootUser.dateOfBirth.year,
                Email = response.Data.rootUser.email,
                PhoneNumber = $"{response.Data.rootUser.mobile.countryCode}{response.Data.rootUser.mobile.number}",
                FirstName = response.Data.rootUser.name,
                LastName = response.Data.rootUser.surname,
                Id = response.Data.id.id,
                UserId = loggedInUserId
            };

            _appDbContext.WeavrCorporateUsers.Add(corporateUser);
            await _appDbContext.SaveChangesAsync();

            return response;
        }

        /// <summary>
        /// This method sends a magic link via email to verify a corporater user's email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<ApiResponse<string>> InitiateEmailVerificationForCorporates(string email)
        {
            bool emailExists = await _appDbContext.WeavrCorporateUsers.AnyAsync(x => x.Email == email && !x.IsEmailVerified);

            if (!emailExists)
            {
                return new ApiResponse<string>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Email does not exist for corporate user"
                };
            }
            
            var body = new
            {
                email = email
            };

            var url = _config["WeavrEndpoints:SendEmailVerificationCodeForCorporates"];
            var response = await _httpClient.MakeGenericRequestCall<string>(Method.Post, url!, body, null);
            return response;
        }

        /// <summary>
        /// This method is used to verify a corporater user's email
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<string>> VerifyEmailForCorporates(VerifyEmail model)
        {
            if (string.IsNullOrWhiteSpace(model.email) || string.IsNullOrWhiteSpace(model.verificationCode))
            {
                return new ApiResponse<string>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var corporateUser = await _appDbContext.WeavrCorporateUsers.SingleAsync(x => x.Email == model.email && !x.IsEmailVerified);

            if (corporateUser is null)
            {
                return new ApiResponse<string>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Email does not exist for corporate user"
                };
            }

            var url = _config["WeavrEndpoints:VerifyEmailForCorporates"];
            var response = await _httpClient.MakeGenericRequestCall<string>(Method.Post, url!, model, null);

            if (response.ResponseCode != "200")
            {
                return response;
            }

            corporateUser.IsEmailVerified = true;
            await _appDbContext.SaveChangesAsync();
            response.ResponseMessage = "Verified successfully";
            return response;
        }

        /// <summary>
        /// This method is used to create password for newly onboarded users
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<PasswordResponse>> CreatePassword(PasswordRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.Password))
            {
                return new ApiResponse<PasswordResponse>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var corporateUser = await _appDbContext.WeavrCorporateUsers.SingleOrDefaultAsync(x => x.UserId == model.UserId);
            if (corporateUser is null)
            {
                return new ApiResponse<PasswordResponse>
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found"
                };
            }

            var url = _config["WeavrEndpoints:CreatePassword"]!.Replace("{userid}", corporateUser.Id);
            var body = new
            {
                password = new
                {
                    value = model.Password
                }                
            };

            var response = await _httpClient.MakeGenericRequestCall<PasswordResponse>(Method.Post, url, body, null);


            if (response.ResponseCode != "200")
            {
                return response;
            }

            _appDbContext.WeavrLoginCredential.Add(new WeavrLoginCredential
            {
                Password = model.Password,
                UserId = model.UserId,
                Email = corporateUser.Email,
            });
            await _appDbContext.SaveChangesAsync();

            return response;
        }

        /// <summary>
        /// This method logs the user in and returns a token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<PasswordResponse>> LoginWithPassword(LoginRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return new ApiResponse<PasswordResponse>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var body = new
            {
                email = model.Email,
                password = new
                {
                    value = model.Password
                }
            };

            var url = _config["WeavrEndpoints:LoginWithPassword"];
            var response = await _httpClient.MakeGenericRequestCall<PasswordResponse>(Method.Post, url!, body, null);
            return response;
        }

        /// <summary>
        /// This method begins the Know Your Business Verification Process
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<ApiResponse<KYBResponse>> StartKYB(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new ApiResponse<KYBResponse>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var url = _config["WeavrEndpoints:KYB"];
            var response = await _httpClient.MakeGenericRequestCall<KYBResponse>(Method.Post, url!, null, token);
            return response;
        }

        /// <summary>
        /// This method gets the Know Your Business Verification Status
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<ApiResponse<KYBStatusResponse>> GetKYBStatus(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new ApiResponse<KYBStatusResponse>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide token on header"
                };
            }

            var url = _config["WeavrEndpoints:KYB"];
            var response = await _httpClient.MakeGenericRequestCall<KYBStatusResponse>(Method.Get, url!, null, token);

            if (response.ResponseCode != "200")
            {
                return response;
            }

            if (response.Data!.KybStatus == "APPROVED")
            {
                string loggedInUserId = _httpContextAccessor.HttpContext!.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                var corporateUser = await _appDbContext.WeavrCorporateUsers.SingleOrDefaultAsync(x => !x.IsOnboardingCompleted && x.UserId == loggedInUserId);

                if (corporateUser is not null)
                {
                    corporateUser.IsOnboardingCompleted = true;
                    await _appDbContext.SaveChangesAsync();
                }
            }

            return response;
        }

        /// <summary>
        /// This method sends out verification code as OTP to user's phone number.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<ApiResponse<string>> SendVerificationCodeFor2FA(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new ApiResponse<string>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var url = _config["WeavrEndpoints:EnrolFor2FA"]!.Replace("{channel}", "SMS");
            var response = await _httpClient.MakeGenericRequestCall<string>(Method.Post, url!, null, token);
            return response;
        }

        /// <summary>
        /// This verifies a corporate user's phone number
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<string>> VerifyCodeFor2FA(string token, StepUpVerification model)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(model.verificationCode))
            {
                return new ApiResponse<string>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var url = _config["WeavrEndpoints:VerifyCodeFor2FA"]!.Replace("{channel}", "SMS");
            var response = await _httpClient.MakeGenericRequestCall<string>(Method.Post, url!, model, token);

            if (response.ResponseCode != "200")
            {
                return response;
            }

            string loggedInUserId = _httpContextAccessor.HttpContext!.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var corporateUser = await _appDbContext.WeavrCorporateUsers.SingleOrDefaultAsync(x => x.UserId == loggedInUserId && !x.IsPhoneNumberVerified);
            
            if (corporateUser is not null)
            {
                corporateUser.IsPhoneNumberVerified = true;
                await _appDbContext.SaveChangesAsync();
            }

            return response;
        }

        /// <summary>
        /// This method creates a managed account for weavr user
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<ApiResponse<ManagedAccountResponse>> CreateManagedAccount(CreateManagedAccount model, string token)
        {
            if (string.IsNullOrWhiteSpace(model.Currency) || string.IsNullOrWhiteSpace(model.FriendlyName) || string.IsNullOrWhiteSpace(token))
            {
                return new ApiResponse<ManagedAccountResponse>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var body = new
            {
                profileId = _config["WeavrProfiles:ManagedAccounts"]!,
                friendlyName = model.FriendlyName,
                currency = model.Currency
            };

            // idem ref
            var idempotencyRef = DateTime.UtcNow + Guid.NewGuid().ToString();
            var url = _config["WeavrEndpoints:ManagedAccounts"]!;

            var response = await _httpClient.MakeGenericRequestCall<ManagedAccountResponse>(Method.Post, url!, body, token, idempotencyRef);

            if (response.ResponseCode != "200")
            {
                return response;
            }

            WeavrCorporateAccount account = new WeavrCorporateAccount
            {
                Currency = response.Data.currency,
                FriendlyName = response.Data.friendlyName,
                ProfileId = response.Data.profileId,
                State = response.Data.state.state
            };

            _appDbContext.WeavrCorporateAccounts.Add(account);

            _appDbContext.WeavrAccountBalances.Add(new WeavrAccountBalance
            {
                AccountId = account.Id,
                ActualBalance = response.Data.balances.actualBalance,
                AvailableBalance = response.Data.balances.availableBalance
            });

            await _appDbContext.SaveChangesAsync();

            return response;
        }
    }
}
