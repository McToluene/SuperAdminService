using Configurations.Utility;
using Microsoft.EntityFrameworkCore;
using SuperAdmin.Service.Database;
using SuperAdmin.Service.Database.Entities;
using SuperAdmin.Service.Extensions;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos;
using SuperAdmin.Service.Models.Dtos.RuleEngineDomain;
using SuperAdmin.Service.Models.Enums;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Services.Implementations
{
    public class SalaryDisbursementChargeService : ISalaryDisbursementChargeService
    {
        private readonly AppDbContext _appDbContext;

        public SalaryDisbursementChargeService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        /// <summary>
        /// This method gets salary disbursement charge rules for various companies
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<ApiResponse<PaginationResult<CompanySalaryDisbursementChargeDto>>> GetCompanyRulesForSalaryDisbursementCharge(int page, int pageSize)
        {
            var companyChargeSettings = _appDbContext.CompanySalaryDisbursementChargeSettings
                                                                                                                            .Include(x => x.SalaryDisbursementChargeSetting)
                                                                                                                            .OrderByDescending(x => x.CreatedAt)
                                                                                                                            .AsQueryable();

            var data = await PaginationHelper.PaginateRecords(companyChargeSettings, page, pageSize);
            var dataItems = MapToCompanySalaryDisbursementChargeDto(data.Items);

            return new ApiResponse<PaginationResult<CompanySalaryDisbursementChargeDto>>
            {
                ResponseCode = "200",
                Data = new PaginationResult<CompanySalaryDisbursementChargeDto>(dataItems, data.TotalSize, data.PageNumber, data.PageSize, data.TotalPages)
            };
        }

        /// <summary>
        /// This method gets the general rule for salary disbursement percentage charge
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<GeneralSalaryDisbursementChargeDto>> GetGeneralRuleForSalaryDisbursementCharge()
        {
            var chargeSetting = await _appDbContext.SalaryDisbursementChargeSettings.FirstOrDefaultAsync(x => x.SettingType == SalaryDisbursementSettingType.General);
            if (chargeSetting == null)
            {
                return new ApiResponse<GeneralSalaryDisbursementChargeDto>
                {
                    ResponseCode = "404",
                    ResponseMessage = "General charge setting not found"
                };
            }

            var data = new GeneralSalaryDisbursementChargeDto
            {
                ChargeDeductionPeriod = chargeSetting.ChargeDeductionPeriod,
                Percentage = chargeSetting.Percentage
            };
            return new ApiResponse<GeneralSalaryDisbursementChargeDto>
            {
                ResponseCode = "200",
                Data = data
            };
        }

        /// <summary>
        /// This method gets the next charge deduction date for salary disbursement
        /// </summary>
        /// <param name="id"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public async Task<ApiResponse<NextChargeDateDto>> GetNextChargeDateForSalaryDisbursement(Guid id, ChargeDeductionPeriod period)
        {
            if (!Enum.IsDefined(typeof(ChargeDeductionPeriod), period))
            {
                return new ApiResponse<NextChargeDateDto>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide a valid charge deduction period"
                };
            }

            var companyChargeSetting = await _appDbContext.CompanySalaryDisbursementChargeSettings.FirstOrDefaultAsync(x => x.Id == id);
            if (companyChargeSetting is null)
            {
                return new ApiResponse<NextChargeDateDto>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Company not found"
                };
            }

            DateTime nextChargeDeductionDate = GetNextChargeDeductionDate(period, companyChargeSetting.SalaryPaymentDate);
            return new ApiResponse<NextChargeDateDto>
            {
                ResponseCode = "200",
                Data = new NextChargeDateDto
                {
                    NextChargeDateInEpochMilliseconds = nextChargeDeductionDate.ToEpochTimestampInMilliseconds()
                }
            };
        }

        /// <summary>
        /// This method updates company rule for salary disbursement charge
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> UpdateCompanyRuleForSalaryDisbursementCharge(Guid id, UpdateSalaryDisbursementCharge model)
        {
            if (model is null)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide parameters to update"
                };
            }

            if (!(model.Percentage >= 0 && model.Percentage <= 100))
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Percentage cannot be less than 0 or more than 100"
                };
            }

            if (!Enum.IsDefined(typeof(ChargeDeductionPeriod), model.ChargeDeductionPeriod))
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide a valid charge deduction period"
                };
            }

            var companyChargeSetting = await _appDbContext.CompanySalaryDisbursementChargeSettings.Include(x => x.SalaryDisbursementChargeSetting)
                                                                                                                                .FirstOrDefaultAsync(x => x.Id == id);
            if (companyChargeSetting is null)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "404",
                    ResponseMessage = "Company not found"
                };
            }

            switch (companyChargeSetting.SettingStatus)
            {
                case SalaryDisbursementSettingStatus.Default:
                    
                    Guid customSettingsId = CreateCustomSalaryDisbursementChargeSetting(model.ChargeDeductionPeriod, model.Percentage);
                    companyChargeSetting.SalaryDisbursementChargeSettingId = customSettingsId;
                    companyChargeSetting.SettingStatus = SalaryDisbursementSettingStatus.Modified;
                    companyChargeSetting.IsNewRecord = false;
                    break;

                case SalaryDisbursementSettingStatus.Modified:
                    companyChargeSetting.SalaryDisbursementChargeSetting.ChargeDeductionPeriod = model.ChargeDeductionPeriod;
                    companyChargeSetting.SalaryDisbursementChargeSetting.Percentage = model.Percentage;
                    companyChargeSetting.SalaryDisbursementChargeSetting.ModifiedAt = DateTime.UtcNow;
                    break;
                
                default:
                    return new ApiResponse<dynamic>
                    {
                        ResponseCode = "409",
                        ResponseMessage = $"Company has an invalid {nameof(companyChargeSetting.SettingStatus)}"
                    };
            }

            companyChargeSetting.NextChargeDate = GetNextChargeDeductionDate(model.ChargeDeductionPeriod, companyChargeSetting.SalaryPaymentDate);
            companyChargeSetting.ModifiedAt = DateTime.UtcNow;
            await _appDbContext.SaveChangesAsync();

            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = $"Updated salary disbursement percentage charge for {companyChargeSetting.CompanyName}"
            };
        }

        /// <summary>
        /// This method updates the general rule for salary disbursement percentage charge
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<dynamic>> UpdateGeneralRuleForSalaryDisbursementCharge(UpdateSalaryDisbursementCharge model)
        {
            if (!(model.Percentage >= 0 && model.Percentage <= 100))
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Percentage cannot be less than 0 or more than 100"
                };
            }

            if (!Enum.IsDefined(typeof(ChargeDeductionPeriod), model.ChargeDeductionPeriod))
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide a valid charge deduction period"
                };
            }

            var chargeSetting = await _appDbContext.SalaryDisbursementChargeSettings.FirstOrDefaultAsync(x => x.SettingType == SalaryDisbursementSettingType.General);
            if (chargeSetting is null)
            {
                return new ApiResponse<dynamic>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please create a general salary disbursement charge setting first"
                };
            }

            chargeSetting.Percentage = model.Percentage;
            chargeSetting.ChargeDeductionPeriod = model.ChargeDeductionPeriod;
            chargeSetting.ModifiedAt = DateTime.UtcNow;

            await _appDbContext.SaveChangesAsync();
            return new ApiResponse<dynamic>
            {
                ResponseCode = "200",
                ResponseMessage = "Rule engine updated successfully for salary disbursement charge"
            };
        }

        /// <summary>
        /// This method enrols a Pays company's scheduled salary payment date
        /// </summary>
        /// <param name="companyName"></param>
        /// <param name="salaryPaymentDate"></param>
        /// <param name="PaysCompanyId"></param>
        /// <returns></returns>
        public async Task<ApiResponse<string>> EnrolScheduledCompanySalaryPaymentDate(string companyName, DateTime salaryPaymentDate, string PaysCompanyId)
        {
            if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(PaysCompanyId) || salaryPaymentDate == new DateTime())
            {
                return new ApiResponse<string>
                {
                    ResponseCode = "400",
                    ResponseMessage = "Please provide required parameters"
                };
            }

            var defaultChargeSettingId = await _appDbContext.SalaryDisbursementChargeSettings.Where(x => x.SettingType == SalaryDisbursementSettingType.General).Select(x => x.Id).FirstOrDefaultAsync();
            var companyChargeSetting = await _appDbContext.CompanySalaryDisbursementChargeSettings.FirstOrDefaultAsync(x => x.PaysCompanyId == PaysCompanyId && x.CompanyName == companyName);
            if (companyChargeSetting is not null)
            {
                companyChargeSetting.SalaryPaymentDate = salaryPaymentDate;
                companyChargeSetting.ModifiedAt = DateTime.UtcNow;
                companyChargeSetting.NextChargeDate = DateTime.UtcNow;
                companyChargeSetting.IsNewRecord = false;

                _appDbContext.CompanySalaryDisbursementChargeSettings.Update(companyChargeSetting);
                await _appDbContext.SaveChangesAsync();

                return new ApiResponse<string>
                {
                    ResponseCode = "200",
                    Data = companyChargeSetting.Id.ToString()
                };
            }

            companyChargeSetting = new CompanySalaryDisbursementChargeSetting
            {
                PaysCompanyId = PaysCompanyId,
                CompanyName = companyName,
                IsNewRecord = true,
                SettingStatus = SalaryDisbursementSettingStatus.Default,
                SalaryPaymentDate = salaryPaymentDate,
                NextChargeDate = salaryPaymentDate,
                SalaryDisbursementChargeSettingId = defaultChargeSettingId
            };
            
            _appDbContext.CompanySalaryDisbursementChargeSettings.Add(companyChargeSetting);
            await _appDbContext.SaveChangesAsync();

            return new ApiResponse<string>
            {
                ResponseCode = "200",
                Data = companyChargeSetting.Id.ToString()
            };
        }

        /// <summary>
        /// This private method maps data to dto
        /// </summary>
        /// <returns></returns>
        private CompanySalaryDisbursementChargeDto[] MapToCompanySalaryDisbursementChargeDto(CompanySalaryDisbursementChargeSetting[] data)
        {
            List<CompanySalaryDisbursementChargeDto> dtos = new();

            foreach (var item in data)
            {
                bool isNewRecord = item.IsNewRecord;
                if (item.IsNewRecord)
                {
                    var afterOneMonth = item.CreatedAt.AddMonths(1);
                    if (DateTime.UtcNow > afterOneMonth)
                    {
                        isNewRecord = false;
                    }
                }

                var dto = new CompanySalaryDisbursementChargeDto
                {
                    Id = item.Id,
                    CompanyName = item.CompanyName,
                    Status = item.SettingStatus,
                    ChargePercentage = item.SalaryDisbursementChargeSetting.Percentage,
                    ChargeDeductionPeriod = item.SalaryDisbursementChargeSetting.ChargeDeductionPeriod,
                    SalaryPaymentDateInEpochMilliseconds = item.SalaryPaymentDate.ToEpochTimestampInMilliseconds(),
                    NextChargeDateInEpochMilliseconds = item.NextChargeDate.ToEpochTimestampInMilliseconds(),
                    IsNewRecord = isNewRecord
                };
                dtos.Add(dto);
            }

            return dtos.ToArray();
        }

        /// <summary>
        /// This private method creates a custom salary disbursement charge setting record for a Pays company
        /// </summary>
        /// <param name="period"></param>
        /// <param name="percentage"></param>
        /// <returns></returns>
        private Guid CreateCustomSalaryDisbursementChargeSetting(ChargeDeductionPeriod period, double percentage)
        {
            var newRecord = new SalaryDisbursementChargeSetting
            {
                SettingType = SalaryDisbursementSettingType.Custom,
                ChargeDeductionPeriod = period,
                Percentage = percentage
            };

            _appDbContext.Add(newRecord);
            return newRecord.Id;
        }

        /// <summary>
        /// This private method calculates the next charge deduction date
        /// </summary>
        /// <param name="period"></param>
        /// <param name="salaryPaymentDate"></param>
        /// <returns></returns>
        private static DateTime GetNextChargeDeductionDate(ChargeDeductionPeriod period, DateTime salaryPaymentDate)
        {
            switch (period)
            {
                case ChargeDeductionPeriod.OnPaymentDay:
                default:
                    return salaryPaymentDate;

                case ChargeDeductionPeriod.Monthly:
                    return salaryPaymentDate.AddMonths(-1);
                
                case ChargeDeductionPeriod.Bimonthly:
                    return salaryPaymentDate.AddMonths(-2);
                
                case ChargeDeductionPeriod.Quarterly:
                    return salaryPaymentDate.AddMonths(-3);
                
                case ChargeDeductionPeriod.Biannually:
                    return salaryPaymentDate.AddMonths(-6);

                case ChargeDeductionPeriod.Yearly:
                    return salaryPaymentDate.AddYears(-1);
            }
        }
    }
}
