using Configurations.Utility;
using Microsoft.EntityFrameworkCore;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Models.Dtos.RuleEngineDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Services.Implementations
{
    public class PayrollVendorChargeService : IPayrollVendorChargeService
    {
        private readonly AppDbContext _appDbContext;

        public PayrollVendorChargeService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// This method creates a new charge deduction basis for payroll vendor charge
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<ApiResponse<ChargeDeductionBasisDto>> CreateChargeDeductionBasis(string name)
        {
            bool doesNameExist = await _appDbContext.PayrollVendorChargeDeductionBasis.AnyAsync(x => x.Name == name);
            
            if (doesNameExist)
            {
                return new ApiResponse<ChargeDeductionBasisDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = $"{name} already exists"
                };
            }

            var chargeDeductionBasis = new PayrollVendorChargeDeductionBasis
            {
                Name = name
            };
            
            _appDbContext.PayrollVendorChargeDeductionBasis.Add(chargeDeductionBasis);
            await _appDbContext.SaveChangesAsync();

            return new ApiResponse<ChargeDeductionBasisDto>
            {
                ResponseCode = "200",
                ResponseMessage = $"{name} created successfully"
            };
        }

        /// <summary>
        /// This method gets the charge deduction basis options for payroll vendor charge
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<List<ChargeDeductionBasisDto>>> GetChargeDeductionBases()
        {
            var data = await _appDbContext.PayrollVendorChargeDeductionBasis.Select(x => new ChargeDeductionBasisDto
            {
                ChargeDeductionBasis = x.Name,
                Id = x.Id
            }).ToListAsync();

            return new ApiResponse<List<ChargeDeductionBasisDto>>
            {
                Data = data,
                ResponseCode = "200"
            };
        }

        /// <summary>
        /// This method gets the payroll vendor charge setting
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<PayrollVendorChargeDto>> GetPayrollVendorCharge()
        {
            var payrollVendorCharge = await _appDbContext.PayrollVendorChargeSetings.Include(x => x.PayrollVendorChargeDeductionBasis)
                                                                .Select(x => new PayrollVendorChargeDto
                                                                {
                                                                    ChargeDeductionBasis = x.PayrollVendorChargeDeductionBasis.Name,
                                                                    Id = x.Id,
                                                                    PercentageCharge = x.Percentage
                                                                }).SingleAsync();

            if (payrollVendorCharge is null)
            {
                return new ApiResponse<PayrollVendorChargeDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please add payroll vendor charge"
                };
            }

            return new ApiResponse<PayrollVendorChargeDto>
            {
                ResponseCode = "200",
                Data = payrollVendorCharge
            };
        }

        /// <summary>
        /// This method updates the payroll vendor charge setting alongside the charge deduction option
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> UpdatePayrollVendorCharge(UpdatePayrollVendorChargeDto model)
        {
            if (model.ChargeDeductionBasisId == Guid.Empty || model.Percentage < 0)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide valid parameters"
                };
            }

            bool chargeDeductionBasisExists = await _appDbContext.PayrollVendorChargeDeductionBasis.AnyAsync(x => x.Id == model.ChargeDeductionBasisId);
            if (!chargeDeductionBasisExists)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Charge deduction basis not found"
                };
            }

            var payrollVendorCharge = await _appDbContext.PayrollVendorChargeSetings.SingleAsync();
            if (payrollVendorCharge is null)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Please add payroll vendor charge first"
                };
            }

            payrollVendorCharge.ModifiedAt = DateTime.UtcNow;
            payrollVendorCharge.ChargeDeductionBasisId = model.ChargeDeductionBasisId;
            payrollVendorCharge.Percentage = model.Percentage;

            await _appDbContext.SaveChangesAsync();

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = "Payroll vendor charge updated successfully"
            };
        }
    }
}
