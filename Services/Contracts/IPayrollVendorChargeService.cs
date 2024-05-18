using Configurations.Utility;
using SuperAdmin.Service.Models.Dtos.RuleEngineDomain;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface IPayrollVendorChargeService
    {
        Task<ApiResponse<PayrollVendorChargeDto>> GetPayrollVendorCharge();
        Task<ApiResponse<dynamic>> UpdatePayrollVendorCharge(UpdatePayrollVendorChargeDto model);
        Task<ApiResponse<ChargeDeductionBasisDto>> CreateChargeDeductionBasis(string name);
        Task<ApiResponse<List<ChargeDeductionBasisDto>>> GetChargeDeductionBases();
    }
}
