using Configurations.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Models.Dtos.AuthDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// This endpoint validates that a user credentials exist at any of the admin roles
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("login/validate-credentials")]
        [ProducesResponseType(typeof(ApiResponse<ValidatedLoginCredentials>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ValidatedLoginCredentials>), 400)]
        [ProducesResponseType(typeof(ApiResponse<ValidatedLoginCredentials>), 409)]
        [ProducesResponseType(typeof(ApiResponse<ValidatedLoginCredentials>), 401)]
        public async Task<IActionResult> ValidateLoginCredentials([FromBody] LoginCredentials model)
        {
            var response = await _authService.ValidateUserLoginCredentials(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint creates a new pin for a newly registered admin
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("pin")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 201)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 401)]
        public async Task<IActionResult> CreatePin([FromBody] CreatePin model)
        {
            var response = await _authService.CreatePinForNewUser(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint validates that a user's pin is correct
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("login/validate-pin")]
        [ProducesResponseType(typeof(ApiResponse<BasicUserData>), 200)]
        [ProducesResponseType(typeof(ApiResponse<BasicUserData>), 400)]
        [ProducesResponseType(typeof(ApiResponse<BasicUserData>), 404)]
        public async Task<IActionResult> ValidateLoginPin([FromBody] ValidatePin model)
        {
            var response = await _authService.ValidatePin(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint validates that the login OTP is correct, and logs user in
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("login/validate-otp")]
        [ProducesResponseType(typeof(ApiResponse<AuthenticatedUser>), 200)]
        [ProducesResponseType(typeof(ApiResponse<AuthenticatedUser>), 400)]
        [ProducesResponseType(typeof(ApiResponse<AuthenticatedUser>), 404)]
        public async Task<IActionResult> LoginUserWithOTP([FromBody] ValidateLoginOTP model)
        {
            var response = await _authService.LoginWithValidatedOtp(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint sends login otp to a user's registered phone number
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("login/send-otp")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        public async Task<IActionResult> SendLoginOTP([FromBody] SendLoginOTP model)
        {
            var response = await _authService.SendLoginOtp(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint sends email notification for password reset token
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost("password/reset/email/{email}")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        public async Task<IActionResult> SendPasswordResetToken([FromRoute] string email)
        {
            var response = await _authService.SendPasswordResetToken(email);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint resets a user's password with a new one.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("password/reset")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var response = await _authService.ResetPassword(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint sends pin rest otp through sms
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("pin/reset/send-otp")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        public async Task<IActionResult> SendPinResetOtp([FromBody] SendPinResetOTP model)
        {
            var response = await _authService.SendPinResetOtp(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint resets a user's pin with a new one.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("pin/reset")]
        [ProducesResponseType(typeof(ApiResponse<AuthenticatedUser>), 200)]
        [ProducesResponseType(typeof(ApiResponse<AuthenticatedUser>), 400)]
        [ProducesResponseType(typeof(ApiResponse<AuthenticatedUser>), 404)]
        public async Task<IActionResult> ResetPin([FromBody] ResetPinDto model)
        {
            var response = await _authService.ResetPin(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint generates a random password
        /// </summary>
        /// <returns></returns>
        [HttpGet("password")]
        [ProducesResponseType(typeof(ApiResponse<GeneratedPassword>), 200)]
        public IActionResult GeneratePassword()
        {
            var response = _authService.GeneratePassword();
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint creates a new password only for users that have expired passwords
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("password")]
        [ProducesResponseType(typeof(ApiResponse<ValidatedLoginCredentials>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ValidatedLoginCredentials>), 400)]
        [ProducesResponseType(typeof(ApiResponse<ValidatedLoginCredentials>), 409)]
        [ProducesResponseType(typeof(ApiResponse<ValidatedLoginCredentials>), 401)]
        public async Task<IActionResult> CreateNewPassword([FromBody] CreatePassword model)
        {
            var response = await _authService.CreateNewPassword(model);
            return ParseResponse(response);
        }
    }
}
