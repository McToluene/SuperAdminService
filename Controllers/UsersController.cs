using Configurations.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos;
using SuperAdmin.Service.Models.Dtos.AuthDomain;
using SuperAdmin.Service.Models.Dtos.UserDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// This endpoint gets users
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginationResult<UserDto>>), 200)]
        public async Task<IActionResult> GetUsers([FromQuery] FilterUserBy model)
        {
            var response = await _userService.GetUsers(model);
            return ParseResponse(response);
        }
        
        /// <summary>
        /// This endpoint gets suspended users
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("suspended")]
        [ProducesResponseType(typeof(ApiResponse<PaginationResult<UserDto>>), 200)]
        public async Task<IActionResult> GetSuspendedUsers([FromQuery] FilterUserBy model)
        {
            var response = await _userService.GetSuspendedUsers(model);
            return ParseResponse(response);
        }
        
        /// <summary>
        /// This endpoint gets suspended users
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet("deleted")]
        [ProducesResponseType(typeof(ApiResponse<PaginationResult<UserDto>>), 200)]
        public async Task<IActionResult> GetDeletedUsers([FromQuery] FilterUserBy model)
        {
            var response = await _userService.GetDeletedUsers(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint creates a new user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUser model)
        {
            var response = await _userService.CreateUser(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint updates user details
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPatch("{userId}")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        public async Task<IActionResult> UpdateUser([FromRoute] string userId, [FromBody] UpdateUser model)
        {
            var response = await _userService.UpdateUser(userId, model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint sets a new password for a user by a superadmin
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPatch("{userId}/password/reset")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        public async Task<IActionResult> ResetPassword([FromRoute] string userId, [FromBody] GeneratedPassword password)
        {
            var response = await _userService.ResetUserPassword(userId, password.Password);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint suspends users
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("suspend")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        public async Task<IActionResult> SuspendUsers([FromBody] SuspendUser model)
        {
            var response = await _userService.SuspendUsers(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint reactivates suspended users
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        [HttpPost("reactivate")]
        public async Task<IActionResult> ReactivateSuspendedUsers([FromBody] ReactivateUser model)
        {
            var response = await _userService.ReactivateSuspendedUsers(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint deletes a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string userId)
        {
            var response = await _userService.DeleteUser(userId);
            return ParseResponse(response);
        }
    }
}
