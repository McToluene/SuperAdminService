using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos.WeavrDomain;
using SuperAdmin.Service.Services.Contracts;
using System.Net;

namespace SuperAdmin.Service.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class WeavrOnboardingController : BaseController
    {
        private readonly IWeavrClientService _weavrClientService;

        public WeavrOnboardingController(IWeavrClientService weavrClientService)
        {
            _weavrClientService = weavrClientService;
        }

        [HttpPost("corporates")]
        public async Task<IActionResult> CreateCorporateIdentity(CorporateIdentityRequest model)
        {
            var response = await _weavrClientService.CreateCorporateIdentity(model);
            return ParseResponse(response);
        }

        [HttpPost("corporates/email-verification/send-code/{email}")]
        public async Task<IActionResult> SendEmailVerificationCodeForCorporates(string email)
        {
            var response = await _weavrClientService.InitiateEmailVerificationForCorporates(email);
            return ParseResponse(response);
        }

        [AllowAnonymous]
        [HttpGet("corporates/email-verification/verify")]
        public async Task<IActionResult> EmailVerificationForCorporates([FromQuery] string email, [FromQuery] string nonce)
        {
            var response = await _weavrClientService.VerifyEmailForCorporates(new VerifyEmail { email = email, verificationCode = nonce});
            return ParseResponse(response);
        }

        [HttpPost("password")]
        public async Task<IActionResult> CreatePassword(PasswordRequest model)
        {
            var response = await _weavrClientService.CreatePassword(model);
            return ParseResponse(response);
        }

        [HttpPost("kyb")]
        public async Task<IActionResult> StartKYB([FromHeader] string token)
        {
            var response = await _weavrClientService.StartKYB(token);
            return ParseResponse(response);
        }

        [HttpGet("kyb")]
        public async Task<IActionResult> GetKYBStatus([FromHeader] string token)
        {
            var response = await _weavrClientService.GetKYBStatus(token);
            return ParseResponse(response);
        }

        [HttpPost("2FA/enrol")]
        public async Task<IActionResult> EnrolFor2FA([FromHeader] string token)
        {
            var response = await _weavrClientService.SendVerificationCodeFor2FA(token);
            return ParseResponse(response);
        }

        [HttpPost("2FA/verify")]
        public async Task<IActionResult> VerifyCodeFor2FA([FromHeader] string token, [FromBody] StepUpVerification model)
        {
            var response = await _weavrClientService.VerifyCodeFor2FA(token, model);
            return ParseResponse(response);
        }
    }
}
