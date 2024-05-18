using Configurations.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos;
using SuperAdmin.Service.Models.Dtos.RuleEngineDomain;
using SuperAdmin.Service.Models.Enums;
using SuperAdmin.Service.Services.Contracts;
using System.Security.Claims;

namespace SuperAdmin.Service.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RuleEngineController : BaseController
    {
        private readonly ISalaryDisbursementChargeService _salaryDisbursementChargeService;
        private readonly IPayrollVendorChargeService _payrollVendorChargeService;
        private readonly IAutoSavingService _autoSavingService;

        public RuleEngineController(ISalaryDisbursementChargeService salaryDisbursementChargeService, IPayrollVendorChargeService payrollVendorChargeService, IAutoSavingService autoSavingService)
        {
            _salaryDisbursementChargeService = salaryDisbursementChargeService;
            _payrollVendorChargeService = payrollVendorChargeService;
            _autoSavingService = autoSavingService;
        }

        /// <summary>
        /// This endpoint gets the general rule for salary disbursement percentage charge
        /// </summary>
        /// <returns></returns>
        [HttpGet("salary-disbursement-charge/general")]
        [ProducesResponseType(typeof(ApiResponse<GeneralSalaryDisbursementChargeDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<GeneralSalaryDisbursementChargeDto>), 404)]
        public async Task<IActionResult> GetGeneralRuleForSalaryDisbursementCharge()
        {
            var response = await _salaryDisbursementChargeService.GetGeneralRuleForSalaryDisbursementCharge();
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint updates the general rule for salary disbursement percentage charge
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPatch("salary-disbursement-charge/general")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        public async Task<IActionResult> UpdateGeneralRuleForSalaryDisbursementCharge([FromBody] UpdateSalaryDisbursementCharge model)
        {
            var response = await _salaryDisbursementChargeService.UpdateGeneralRuleForSalaryDisbursementCharge(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint gets salary disbursement charge rules for companies
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("salary-disbursement-charge/companies")]
        [ProducesResponseType(typeof(ApiResponse<CompanySalaryDisbursementChargeDto>), 200)]
        public async Task<IActionResult> GetCompanyRulesForSalaryDisbursementCharge([FromQuery] Pagination model)
        {
            var response = await _salaryDisbursementChargeService.GetCompanyRulesForSalaryDisbursementCharge(model.Page.Value, model.PageSize.Value);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint updates company rule for salary disbursement charge
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPatch("salary-disbursement-charge/companies/{id}")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 409)]
        public async Task<IActionResult> UpdateCompanyRuleForSalaryDisbursementCharge([FromRoute] Guid id, [FromBody] UpdateSalaryDisbursementCharge model)
        {
            var response = await _salaryDisbursementChargeService.UpdateCompanyRuleForSalaryDisbursementCharge(id, model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint gets the next charge deduction date for salary disbursement
        /// </summary>
        /// <param name="id"></param>
        /// <param name="chargeDeductionPeriod"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApiResponse<CompanySalaryDisbursementChargeDto>), 200)]
        [HttpGet("salary-disbursement-charge/companies/{id}/period/{chargeDeductionPeriod}")]
        public async Task<IActionResult> UpdateCompanyRuleForSalaryDisbursementCharge([FromRoute] Guid id, [FromRoute] ChargeDeductionPeriod chargeDeductionPeriod)
        {
            var response = await _salaryDisbursementChargeService.GetNextChargeDateForSalaryDisbursement(id, chargeDeductionPeriod);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint creates a new charge deduction basis for payroll vendor charge
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost("payroll-vendor-charge/charge-deduction-basis/{name}")]
        [ProducesResponseType(typeof(ApiResponse<ChargeDeductionBasisDto>), 200)]
        public async Task<IActionResult> CreateChargeDeductionBasis([FromRoute] string name)
        {
            var response = await _payrollVendorChargeService.CreateChargeDeductionBasis(name);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint gets the charge deduction basis options for payroll vendor charge
        /// </summary>
        /// <returns></returns>
        [HttpGet("payroll-vendor-charge/charge-deduction-basis")]
        [ProducesResponseType(typeof(ApiResponse<List<ChargeDeductionBasisDto>>), 200)]
        public async Task<IActionResult> GetChargeDeductionBasis()
        {
            var response = await _payrollVendorChargeService.GetChargeDeductionBases();
            return ParseResponse(response);
        }
        
        /// <summary>
        /// This endpoint gets the charge deduction basis options for payroll vendor charge
        /// </summary>
        /// <returns></returns>
        [HttpGet("payroll-vendor-charge")]
        [ProducesResponseType(typeof(ApiResponse<PayrollVendorChargeDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<PayrollVendorChargeDto>), 400)]
        public async Task<IActionResult> GetPayrollVendorCharge()
        {
            var response = await _payrollVendorChargeService.GetPayrollVendorCharge();
            return ParseResponse(response);
        }
        
        /// <summary>
        /// This endpoint gets the charge deduction basis options for payroll vendor charge
        /// </summary>
        /// <returns></returns>
        [HttpPatch("payroll-vendor-charge")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        public async Task<IActionResult> UpdatePayrollVendorCharge([FromBody] UpdatePayrollVendorChargeDto model)
        {
            var response = await _payrollVendorChargeService.UpdatePayrollVendorCharge(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint creates a new auto saving setting
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [HttpPost("auto-saving")]
        public async Task<IActionResult> CreateAutoSavingSetting([FromBody] CreateAutoSavingSetting model)
        {
            var response = await _autoSavingService.CreateAutoSavingSetting(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint gets all auto saving settings
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AutoSavingSettingDto>>), 200)]
        [HttpGet("auto-saving")]
        public async Task<IActionResult> GetAutoSavingSettings()
        {
            var response = await _autoSavingService.GetAutoSavingSettings();
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint updates selected auto saving settings
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [HttpPatch("auto-saving")]
        public async Task<IActionResult> GetAutoSavingSettings([FromBody] UpdateAutoSavingSettingDto model)
        {
            string loggedInUser = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var response = await _autoSavingService.UpdateAutoSavingSettings(loggedInUser, model);
            return ParseResponse(response);
        }
    }
}
