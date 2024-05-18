using Configurations.Utility;
using SuperAdmin.Service.Models.Dtos.RuleEngineDomain;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface IAutoSavingService
    {
        Task<ApiResponse<IEnumerable<AutoSavingSettingDto>>> GetAutoSavingSettings();
        Task<ApiResponse<dynamic>> CreateAutoSavingSetting(CreateAutoSavingSetting model);
        Task<ApiResponse<dynamic>> UpdateAutoSavingSettings(string userId, UpdateAutoSavingSettingDto model);
    }
}
