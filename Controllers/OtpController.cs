using Common.Services.Utility;
using Configurations.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Models.Dtos.OtpDomain;
using SuperAdmin.Service.Models.Enums;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class OtpController : BaseController
    {
        private readonly IOtpService _otpService;

        public OtpController(IOtpService otpService)
        {
            _otpService = otpService;
        }

        /// <summary>
        /// Sned otp via sms 
        /// </summary>
        /// <param name="otpType"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("sms/{otpType}")]
        public async Task<IActionResult> SendOtpViaSMS(OtpType otpType, [FromBody] SendOtpDto model)
        {
            (bool isSuccessful, string message, string otpCode) = await _otpService.CreateOtpUsingUserPhoneNumber(model.UserId, model.PhoneNumber, otpType);

            var response = new ApiResponse<dynamic>();
            if (!isSuccessful)
            {
                response.ResponseCode = "400";
                response.ResponseMessage = message;

                return ParseResponse(response);
            }

            string messageBody = $"Use this OTP code: {otpCode} to complete changes to settings.";

            try
            {
                // Todo: Publish an event to send sms for the Notfication service to handle and remove the try and catch block here
                Utility.SendSMS(model.PhoneNumber, messageBody);
            }
            catch (Exception ex)
            {
                response.ResponseCode = "400";
                response.ResponseMessage = $"SMS not delivered. {ex.Message}";

                return ParseResponse(response);
            }

            response.ResponseCode = "200";
            response.ResponseMessage = $"SMS delivered to {model.PhoneNumber}";
            return ParseResponse(response);
        }
    }
}
