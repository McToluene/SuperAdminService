using Common.Services.Utility;
using Configurations.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Extensions;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos.AuthDomain;
using SuperAdmin.Service.Models.Enums;
using SuperAdmin.Service.Services.Contracts;
using System.Text;
using System.Web;

namespace SuperAdmin.Service.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IOtpService _otpService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;
        private const int PIN_MAX_DIGIT = 4;

        public AuthService(
            AppDbContext context,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOtpService otpService,
            ITokenService tokenService,
            IConfiguration config)
        {
            _appDbContext = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _otpService = otpService;
            _tokenService = tokenService;
            _config = config;
        }

        #region Validate User Login Credentials
        /// <summary>
        /// This method validates user's email against the password provided
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<ValidatedLoginCredentials>> ValidateUserLoginCredentials(LoginCredentials model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                return new ApiResponse<ValidatedLoginCredentials>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide login credentials"
                };
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted)
            {
                return new ApiResponse<ValidatedLoginCredentials>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Invalid login credentials"
                };
            }

            if (!user.HasPasswordChanged && user.PasswordExpiryDate < DateTime.UtcNow)
            {
                return new ApiResponse<ValidatedLoginCredentials>
                {
                    ResponseCode = "409",
                    ResponseMessage = "Password has expired. Please create a new password",
                    Data = new ValidatedLoginCredentials
                    {
                        UserId = user.Id
                    }
                };
            }

            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                return new ApiResponse<ValidatedLoginCredentials>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Invalid login credentials"
                };
            }

            var adminRoles = await _roleManager.Roles.Where(x => x.Name != HelperConstants.SUSPENSION_ROLE).Select(x => x.Name).ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var userAdminRoles = adminRoles.Intersect(userRoles);
            if (!userAdminRoles.Any() || userRoles.Contains(HelperConstants.SUSPENSION_ROLE))
            {
                return new ApiResponse<ValidatedLoginCredentials>
                {
                    ResponseCode = "401",
                    ResponseMessage = "Unauthorized"
                };
            }

            return new ApiResponse<ValidatedLoginCredentials>
            {
                ResponseCode = "200",
                Data = new ValidatedLoginCredentials
                {
                    Email = user.Email ?? string.Empty,
                    IsPinSet = user.IsPinSet,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    UserId = user.Id
                }
            };
        }
        #endregion

        #region Create Pin For New User
        /// <summary>
        /// This method creates pin for a new user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> CreatePinForNewUser(CreatePin model)
        {
            if (string.IsNullOrWhiteSpace(model.Pin) || string.IsNullOrWhiteSpace(model.UserId))
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var user = await GetUserById(model.UserId);
            if (user == null || user.IsDeleted)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found",
                };
            }

            if (user.IsPinSet)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "User already set a pin"
                };
            }

            if (model.Pin.Length != PIN_MAX_DIGIT)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Pin must be {PIN_MAX_DIGIT} digits"
                };
            }

            string hashedPin = BCrypt.Net.BCrypt.HashPassword(model.Pin);
            user.PinHash = hashedPin;
            user.IsPinSet = true;

            await _userManager.UpdateAsync(user);
            await _appDbContext.SaveChangesAsync();
            
            return new ApiResponse<dynamic>
            {
                ResponseCode = "201",
                ResponseMessage = "Pin created successfully"
            };
        }
        #endregion

        #region Validate Pin
        /// <summary>
        /// This method validates the pin a user provided
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<BasicUserData>> ValidatePin(ValidatePin model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.Pin))
            {
                return new ApiResponse<BasicUserData>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var user = await GetUserById(model.UserId);
            if (user is null)
            {
                return new ApiResponse<BasicUserData>
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found"
                };
            }

            if (!user.IsPinSet)
            {
                return new ApiResponse<BasicUserData>
                {
                    ResponseCode = "400",
                    ResponseMessage = "User has no pin set"
                };
            }

            bool isPinValid = BCrypt.Net.BCrypt.Verify(model.Pin, user.PinHash);
            if (!isPinValid)
            {
                return new ApiResponse<BasicUserData>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Incorrect pin"
                };
            }

            return new ApiResponse<BasicUserData>
            {
                ResponseCode = "200",
                ResponseMessage = "Pin successfully validated",
                Data = new BasicUserData
                {
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    UserId = user.Id
                }
            };
        }
        #endregion

        #region Send Login OTP
        /// <summary>
        /// This method sends a login otp to user's phone number
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> SendLoginOtp(SendLoginOTP model)
        {
            (bool isSuccessful, string message, string otp) = await _otpService.CreateOtpUsingUserPhoneNumber(model.UserId, model.PhoneNumber, OtpType.Login);
            if (!isSuccessful)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = message,
                };
            }

            string messageBody = $"Your login OTP code: {otp}. This code would expire after 5 minutes. Do not share with anyone.";

            try
            {
                // Todo: Publish an event to send sms for the Notfication service to handle
                Utility.SendSMS(model.PhoneNumber, messageBody);
            }
            catch (Exception ex)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"SMS not delivered. {ex.Message}"
                };
            }

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = $"SMS delivered to {model.PhoneNumber}"
            };
        }
        #endregion

        #region Login With OTP
        /// <summary>
        /// This method validates the login otp and authenticates the user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<AuthenticatedUser>> LoginWithValidatedOtp(ValidateLoginOTP model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.PhoneNumber) || string.IsNullOrWhiteSpace(model.Otp))
            {
                return new ApiResponse<AuthenticatedUser>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide parameters"
                };
            }

            var user = await GetUserById(model.UserId);
            if (user is null)
            {
                return new ApiResponse<AuthenticatedUser>
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found"
                };
            }

            (bool isValidOtp, string message) = await _otpService.ValidateOtpUsingPhoneNumber(model.UserId, model.PhoneNumber, model.Otp, OtpType.Login);
            if (!isValidOtp)
            {
                return new ApiResponse<AuthenticatedUser>
                {
                    ResponseCode = "400",
                    ResponseMessage = message
                };
            }

            (bool isSuccessful, string token) = await _tokenService.GenerateToken(user);
            if (!isSuccessful)
            {
                return new ApiResponse<AuthenticatedUser>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Token service temporarily down"
                };
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var authenticatedUser = user.ToAuthenticatedUser(token, userRoles);

            return new ApiResponse<AuthenticatedUser>
            {
                Data = authenticatedUser,
                ResponseCode = "200",
                ResponseMessage = "Login successful"
            };
        }
        #endregion

        #region Send Password Reset Token
        /// <summary>
        /// This method sends password reset token through email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> SendPasswordResetToken(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide email"
                };
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Email is not linked to any account"
                };
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            (bool isSuccessful, string message) = await SendPasswordResetEmailNotification(token, email);
            if (!isSuccessful)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseMessage = message,
                    ResponseCode = "400"
                };
            }

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = message
            };
        }
        #endregion

        #region Reset Password
        /// <summary>
        /// This method resets a user's password with a token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> ResetPassword(ResetPasswordDto model)
        {
            if (string.IsNullOrWhiteSpace(model.NewPassword) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Token))
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide parameters"
                };
            }

            User? user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsDeleted)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found"
                };
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                StringBuilder errors = new StringBuilder();

                foreach (var error in result.Errors)
                {
                    errors.Append(error.Description);
                }

                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = errors.ToString()
                };
            }

            if (!user.HasPasswordChanged)
            {
                user.HasPasswordChanged = true;
                user.ModifiedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = "Password saved successfully"
            };
        }
        #endregion

        #region Send Pin Reset OTP
        /// <summary>
        /// This method sends pin reset otp to user's phone number
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> SendPinResetOtp(SendPinResetOTP model)
        {
            (bool isSuccessful, string message) = await DoesUserHaveASetPin(model.UserId);
            if (!isSuccessful)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = message
                };
            }

            (isSuccessful, message, string otp) = await _otpService.CreateOtpUsingUserPhoneNumber(model.UserId, model.PhoneNumber, OtpType.PinReset);

            if (!isSuccessful)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = message
                };
            }

            string messageBody = $"Your reset pin OTP code: {otp}. This code would expire after 5 minutes. Do not share with anyone.";

            try
            {
                // Todo: Publish an event to send sms for the Notfication service to handle and remove the try and catch block here
                Utility.SendSMS(model.PhoneNumber, messageBody); 
            }
            catch (Exception ex)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"SMS not delivered. {ex.Message}"
                };
            }

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = $"SMS delivered to {model.PhoneNumber}"
            };
        }
        #endregion

        #region Generate Password 
        /// <summary>
        /// This method generates a random password
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ApiResponse<GeneratedPassword> GeneratePassword()
        {
            string password = PasswordGenerator.GetRandomPassword(16);

            return new ApiResponse<GeneratedPassword>
            {
                ResponseCode = "200",
                Data = new GeneratedPassword
                {
                    Password = password
                }
            };
        }
        #endregion

        #region Reset Pin
        /// <summary>
        /// This method resets a user's pin using otp
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<AuthenticatedUser>> ResetPin(ResetPinDto model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.NewPin) || string.IsNullOrWhiteSpace(model.Otp))
            {
                return new ApiResponse<AuthenticatedUser>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide parameters"
                };
            }
            
            if (model.NewPin.Length != PIN_MAX_DIGIT)
            {
                return new ApiResponse<AuthenticatedUser>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"Pin must be {PIN_MAX_DIGIT} digits"
                };
            }

            var user = await GetUserById(model.UserId);
            if (user is null || user.IsDeleted)
            {
                return new ApiResponse<AuthenticatedUser>
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found"
                };
            }

            if (!string.Equals(user.PhoneNumber, model.PhoneNumber, StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<AuthenticatedUser>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Invalid phone number"
                };
            }

            if (!user.IsPinSet)
            {
                return new ApiResponse<AuthenticatedUser>
                {
                    ResponseCode = "400",
                    ResponseMessage = "You have not set a pin for your account. Please create one instead."
                };
            }

            (bool isSuccessful, string message) = await _otpService.ValidateOtpUsingPhoneNumber(model.UserId, model.PhoneNumber, model.Otp, OtpType.PinReset);
            if (!isSuccessful)
            {
                return new ApiResponse<AuthenticatedUser>
                {
                    ResponseCode = "400",
                    ResponseMessage = message
                };
            }

            string pinHash = BCrypt.Net.BCrypt.HashPassword(model.NewPin);
            user.PinHash = pinHash;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                StringBuilder errors = new StringBuilder();

                foreach (var error in result.Errors)
                {
                    errors.Append(error.Description);
                }

                return new ApiResponse<AuthenticatedUser>
                {
                    ResponseCode = "400",
                    ResponseMessage = errors.ToString()
                };
            }

            (isSuccessful, string token) = await _tokenService.GenerateToken(user);
            if (!isSuccessful) {
                return new ApiResponse<AuthenticatedUser>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Token service temporarily down"
                };
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var authenticatedUser = user.ToAuthenticatedUser(token, userRoles);

            return new ApiResponse<AuthenticatedUser>
            {
                ResponseCode = "200",
                ResponseMessage = "Pin reset successful",
                Data = authenticatedUser
            };
        }
        #endregion

        #region Create New Password
        /// <summary>
        /// This methods creates a new password for a user that have an expired password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<ValidatedLoginCredentials>> CreateNewPassword(CreatePassword model)
        {
            if (string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.UserId))
            {
                return new ApiResponse<ValidatedLoginCredentials>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null || user.IsDeleted)
            {
                return new ApiResponse<ValidatedLoginCredentials>
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found"
                };
            }

            if (!user.HasPasswordChanged && user.PasswordExpiryDate < DateTime.UtcNow)
            {
                string token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!result.Succeeded)
                {
                    return new ApiResponse<ValidatedLoginCredentials>
                    {
                        ResponseCode = "400",
                        ResponseMessage = string.Join(".", result.Errors.Select(x => x.Description))
                    };
                }
            }
            else
            {
                return new ApiResponse<ValidatedLoginCredentials>
                {
                    ResponseCode = "409",
                    ResponseMessage = "You are restricted from creating a new password"
                };
            }

            user.HasPasswordChanged = true;
            user.ModifiedAt = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return new ApiResponse<ValidatedLoginCredentials>
                {
                    ResponseCode = "400",
                    ResponseMessage = string.Join(".", updateResult.Errors.Select(x => x.Description))
                };
            }

            return new ApiResponse<ValidatedLoginCredentials>
            {
                ResponseCode = "200",
                Data = new ValidatedLoginCredentials
                {
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    UserId = user.Id,
                    IsPinSet = user.IsPinSet
                }
            };
        }
        #endregion        

        #region Send Email Notiication for Password 
        /// <summary>
        /// This method prepares token to be sent and then sends it as an email
        /// </summary>
        /// <param name="token"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task<(bool isSuccessful, string message)> SendPasswordResetEmailNotification(string token, string email)
        {
            string encodedToken = HttpUtility.UrlEncode(token);
            string passwordResetUrl = $"{_config["ApplicationHostUrl"]}/auth/reset-password?email={email}&token={encodedToken}";

            string body = $"We're excited to have you back. Click on the link to reset your password<br/>{passwordResetUrl}<br/><br/>Regards,<br/>Job Pro";
            string subject = "Reset Password";

            // Todo: Publish an event to send email which should be handled by the notification service
            bool isSuccessful = await Utility.SendGridSendMail(body, email, subject);
            if (!isSuccessful)
            {
                return (isSuccessful, "Failed to send password reset email");
            }

            return (isSuccessful, "Successfully sent email");
        }
        #endregion

        #region Get User By Id
        /// <summary>
        /// This method gets a user's data
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<User?> GetUserById(string userId)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId && !x.IsDeleted && !x.IsSuspended);
        }
        #endregion

        #region Does User Have A Set Pin?
        private async Task<(bool isSuccessful, string message)> DoesUserHaveASetPin(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return (false, "Please provide required parameters");
            }

            User? user = await GetUserById(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            if (!user.IsPinSet)
            {
                return (false, "You have not set a pin for your account. Please create one instead.");
            }
            
            return (user.IsPinSet, string.Empty);
        }
        #endregion
    }
}
