using Grpc.Core;
using Serilog;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Services.GrpcServices.Server
{
    public class SalaryDisbursementService : GrpcSalaryDisbursementService.GrpcSalaryDisbursementServiceBase
    {
        private readonly ISalaryDisbursementChargeService _salaryDisbursementChargeService;
        private Serilog.ILogger _logger = Log.ForContext<SalaryDisbursementService>();
        
        public SalaryDisbursementService(ISalaryDisbursementChargeService salaryDisbursementChargeService)
        {
            _salaryDisbursementChargeService = salaryDisbursementChargeService;
        }

        public override async Task<EnrolScheduledSalaryPaymentResponse> EnrolScheduledSalaryPaymentDate(EnrolScheduledSalaryPaymentRequest request, ServerCallContext context)
        {
            try
            {
                _logger.Information("Grpc call - ", $"{nameof(EnrolScheduledSalaryPaymentDate)}");
                var response = await _salaryDisbursementChargeService.EnrolScheduledCompanySalaryPaymentDate(request.CompanyName, request.SalaryPaymentDate.ToDateTime(), request.JopPaysCompanyId);
                
                if (response.ResponseCode == "200")
                {
                    return new EnrolScheduledSalaryPaymentResponse
                    {
                        IsSuccessful = true,
                        CompanySalaryDisbursementChargeSettingId = response.Data
                    };
                }
                else
                {
                    return new EnrolScheduledSalaryPaymentResponse
                    {
                        IsSuccessful = false,
                        CompanySalaryDisbursementChargeSettingId = null
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString(), nameof(EnrolScheduledSalaryPaymentDate));
                return new EnrolScheduledSalaryPaymentResponse
                {
                    IsSuccessful = false,
                    CompanySalaryDisbursementChargeSettingId = null
                };
            }
        }
    }
}
