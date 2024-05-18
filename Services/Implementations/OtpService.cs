using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Enums;
using SuperAdmin.Service.Services.Contracts;
using Twilio.Types;

namespace SuperAdmin.Service.Services.Implementations
{
    public class OtpService : IOtpService
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<User> _userManager;

        public OtpService(AppDbContext appDbContext, UserManager<User> userManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
        }

        /// <summary>
        /// This method generates an otp using user's email
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<(bool isSuccessful, string message, string otpCode)> CreateOtpUsingUserEmail(string userId, string email, OtpType otpType)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(userId))
            {
                return (false, "Please provide required parameters", string.Empty);
            }

            User? user = await _userManager.FindByEmailAsync(email);
            if (user == null) {
                return (false, "You are not a registered user", string.Empty);
            }

            if (!string.Equals(user.Id, userId, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Invalid credentials provided", string.Empty);
            }

            await DiscardUnexpiredGeneratedOtpForEmail(user.Id, email);
            string code = await GenerateOtpUsingEmail(email);

            OneTimePassword otp = new OneTimePassword
            {
                Code = code,
                EmailAddress = user.Email,
                UserId = user.Id,
                OtpStatus = OtpStatus.Generated,
                OtpType = otpType
            };
            
            _appDbContext.OneTimePasswords.Add(otp);
            await _appDbContext.SaveChangesAsync();

            return (true, string.Empty, code);
        }

        /// <summary>
        /// This method creates login otp using user's phone number
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public async Task<(bool isSuccessful, string message, string otpCode)> CreateOtpUsingUserPhoneNumber(string userId, string phoneNumber, OtpType otpType)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(userId))
            {
                return (false, "Please provide required parameters", string.Empty);
            }

            User? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "You are not a registered user", string.Empty);
            }

            if (!string.Equals(user.PhoneNumber, phoneNumber, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Invalid credentials provided", string.Empty);
            }

            await DiscardUnexpiredGeneratedOtpForPhoneNumber(user.Id, phoneNumber);
            string code = await GenerateOtpUsingPhoneNumber(phoneNumber);

            OneTimePassword otp = new OneTimePassword
            {
                Code = code,
                PhoneNumber = phoneNumber,
                UserId = user.Id,
                OtpStatus = OtpStatus.Generated,
                OtpType = otpType
            };

            _appDbContext.OneTimePasswords.Add(otp);
            await _appDbContext.SaveChangesAsync();

            return (true, string.Empty, code);
        }

        /// <summary>
        /// This method validates otp that was delivered to a user's phone number
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="otpCode"></param>
        /// <param name="otpType"></param>
        /// <returns></returns>
        public async Task<(bool isValid, string message)> ValidateOtpUsingPhoneNumber(string userId, string phoneNumber, string otpCode, OtpType otpType)
        {
            var otp = await _appDbContext.OneTimePasswords.OrderByDescending(x => x.CreatedAt)
                                                                        .Where(x => x.OtpType == otpType &&
                                                                                x.PhoneNumber == phoneNumber && 
                                                                                x.UserId == userId && 
                                                                                x.OtpStatus == OtpStatus.Generated)
                                                                        .FirstOrDefaultAsync();

            if (otp == null)
            {
                return (false, "Invalid OTP code");
            }

            if (!string.Equals(otp.Code, otpCode, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Invalid OTP code");
            }

            if (DateTime.UtcNow > otp.OtpExpiryDate)
            {
                otp.OtpStatus = OtpStatus.Expired;
                otp.ModifiedAt = DateTime.UtcNow;

                _appDbContext.OneTimePasswords.Update(otp);
                await _appDbContext.SaveChangesAsync();

                return (false, "OTP is expired");
            }

            otp.OtpStatus = OtpStatus.Used;
            otp.ModifiedAt = DateTime.UtcNow;

            _appDbContext.OneTimePasswords.Update(otp);
            await _appDbContext.SaveChangesAsync();

            return (true, string.Empty);
        }

        /// <summary>
        /// This method generates an otp code using email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task<string> GenerateOtpUsingEmail(string email)
        {
            string code = OtpGenerator.GenerateFourDigitCode();

            OneTimePassword? doesGeneratedCodeExist = await _appDbContext.OneTimePasswords.FirstOrDefaultAsync(x => string.Equals(code, x.Code) 
                                                                                                                && string.Equals(x.EmailAddress, email));
            
            if (doesGeneratedCodeExist is not null)
            {
                return await GenerateOtpUsingEmail(email);
            }

            return code;
        }

        /// <summary>
        /// This method generates an otp code using phone number
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        private async Task<string> GenerateOtpUsingPhoneNumber(string phoneNumber)
        {
            string code = OtpGenerator.GenerateFourDigitCode();

            var doesGeneratedCodeExist = await _appDbContext.OneTimePasswords.FirstOrDefaultAsync(x => string.Equals(code, x.Code)
                                                                                                    && string.Equals(x.PhoneNumber, phoneNumber));

            if (doesGeneratedCodeExist is not null)
            {
                return await GenerateOtpUsingPhoneNumber(phoneNumber);
            }

            return code;
        }

        /// <summary>
        /// This method converts all unexpired generated otps sent through SMS to expired otps.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        private async Task DiscardUnexpiredGeneratedOtpForPhoneNumber(string userId, string phoneNumber)
        {
            IQueryable<OneTimePassword>? otps = _appDbContext.OneTimePasswords.Where(x => x.PhoneNumber == phoneNumber && x.UserId == userId && x.OtpStatus == OtpStatus.Generated);

            foreach (var otp in otps)
            {
                otp.OtpStatus = OtpStatus.Expired;
                otp.ModifiedAt = DateTime.UtcNow;
            }

            if (otps.Any())
            {
                await _appDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// This method converts all unexpired generated otps sent through email to expired otps.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task DiscardUnexpiredGeneratedOtpForEmail(string userId, string email)
        {
            IQueryable<OneTimePassword>? otps = _appDbContext.OneTimePasswords.Where(x => x.EmailAddress == email && x.UserId == userId && x.OtpStatus == OtpStatus.Generated);

            foreach (var otp in otps)
            {
                otp.OtpStatus = OtpStatus.Expired;
                otp.ModifiedAt = DateTime.UtcNow;
            }

            if (otps.Any())
            {
                await _appDbContext.SaveChangesAsync();
            }
        }
    }
}
