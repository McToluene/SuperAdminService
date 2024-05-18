using Configurations.Utility;
using SuperAdmin.Service.Models.Dtos;
using SuperAdmin.Service.Models.Dtos.RuleEngineDomain;
using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface ISalaryDisbursementChargeService
    {
        Task<ApiResponse<GeneralSalaryDisbursementChargeDto>> GetGeneralRuleForSalaryDisbursementCharge();
        Task<ApiResponse<dynamic>> UpdateGeneralRuleForSalaryDisbursementCharge(UpdateSalaryDisbursementCharge model);
        Task<ApiResponse<PaginationResult<CompanySalaryDisbursementChargeDto>>> GetCompanyRulesForSalaryDisbursementCharge(int page, int pageSize);
        Task<ApiResponse<dynamic>> UpdateCompanyRuleForSalaryDisbursementCharge(Guid id, UpdateSalaryDisbursementCharge model);
        Task<ApiResponse<NextChargeDateDto>> GetNextChargeDateForSalaryDisbursement(Guid id, ChargeDeductionPeriod period);
        Task<ApiResponse<string>> EnrolScheduledCompanySalaryPaymentDate(string companyName, DateTime salaryPaymentDate, string PaysCompanyId);
    }
}
