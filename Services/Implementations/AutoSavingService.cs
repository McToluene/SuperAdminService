using Configurations.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Models.Dtos.RuleEngineDomain;
using SuperAdmin.Service.Models.Enums;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Services.Implementations
{
    public class AutoSavingService : IAutoSavingService
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<User> _userManager;
        private readonly IOtpService _otpService;

        public AutoSavingService(AppDbContext appDbContext, UserManager<User> userManager, IOtpService otpService)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _otpService = otpService;
        }

        /// <summary>
        /// This method creates a new auto saving setting
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> CreateAutoSavingSetting(CreateAutoSavingSetting model)
        {
            if (model.DurationInMonths < 1 || model.InterestPercentage < 0)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide valid parameters"
                };
            }

            bool doesSettingExist = await _appDbContext.AutoSavingSettings.AnyAsync(x => x.DurationInMonths == model.DurationInMonths);
            if (doesSettingExist)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Duration already exists"
                };
            }

            _appDbContext.AutoSavingSettings.Add(new AutoSavingSetting
            {
                DurationInMonths = model.DurationInMonths,
                Description = $"{model.DurationInMonths} months",
                InterestPercentage = model.InterestPercentage
            });
            await _appDbContext.SaveChangesAsync();

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = "Created successfully"
            };
        }

        /// <summary>
        /// This method returns all auto saving settings
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<IEnumerable<AutoSavingSettingDto>>> GetAutoSavingSettings()
        {
            var settings = await _appDbContext.AutoSavingSettings.OrderBy(x => x.DurationInMonths)
                                                                                        .Select(x => new AutoSavingSettingDto
                                                                                        {
                                                                                            Description = x.Description,
                                                                                            InterestPercentage = x.InterestPercentage,
                                                                                            Id = x.Id
                                                                                        }).ToListAsync();

            return new ApiResponse<IEnumerable<AutoSavingSettingDto>>
            {
                ResponseCode = "200",
                Data = settings
            };
        }

        /// <summary>
        /// This method updates auto saving settings
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> UpdateAutoSavingSettings(string userId, UpdateAutoSavingSettingDto model)
        {
            (bool isSuccessful, string? message) = ValidateUpdateAutoSavingSettingRequest(model);
            if (!isSuccessful)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = message!
                };
            }

            if (userId != model.UserId)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "You cannot update this setting. Please login"
                };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "User not found"
                };
            }

            bool isPinValid = BCrypt.Net.BCrypt.Verify(model.Pin, user.PinHash);
            if (!isPinValid)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Incorrect pin"
                };
            }

            (bool isOtpValid, message) = await _otpService.ValidateOtpUsingPhoneNumber(model.UserId, user.PhoneNumber!, model.Otp, OtpType.AutoSavingSetting);
            if (!isOtpValid)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = message
                };
            }

            bool isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Incorrect password"
                };
            }

            foreach (var item in model.AutoSavingSettings)
            {
                var autoSavingSetting = await _appDbContext.AutoSavingSettings.FirstOrDefaultAsync(x => x.Id == item.AutoSavingSettingId);
                if (autoSavingSetting is null)
                {
                    continue;
                }

                if (autoSavingSetting.InterestPercentage != item.InterestPercentage)
                {
                    autoSavingSetting.InterestPercentage = item.InterestPercentage;
                    autoSavingSetting.ModifiedAt = DateTime.UtcNow;
                }
            }

            await _appDbContext.SaveChangesAsync();
            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = "Auto saving settings updated successfully."
            };
        }

        private static (bool, string?) ValidateUpdateAutoSavingSettingRequest(UpdateAutoSavingSettingDto model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId) ||
                string.IsNullOrWhiteSpace(model.Otp) ||
                string.IsNullOrWhiteSpace(model.Pin) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                model.AutoSavingSettings.IsNullOrEmpty())
            {
                return (false, "Please provide required parameters");
            }

            foreach (var item in model.AutoSavingSettings)
            {
                if (item.InterestPercentage < 0)
                {
                    return (false, "Interest percentage cannot be a negative value");
                }
            }

            return (true, null);
        }
    }
}
