using Configurations.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos.PermissionDomain;
using SuperAdmin.Service.Models.Dtos.RoleDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RolesController : BaseController
    {
        private readonly IPermissionService _permissionService;
        private readonly IRoleService _roleService;

        public RolesController(
            IPermissionService permissionService,
            IRoleService roleService)
        {
            _permissionService = permissionService;
            _roleService = roleService;
        }

        /// <summary>
        /// This endpoint gets all permissions
        /// </summary>
        /// <returns></returns>
        [HttpGet("permissions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PermissionDto>>), 200)]
        public async Task<IActionResult> GetPermissions()
        {
            var response = await _permissionService.GetPermissions();
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint gets all roles
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RoleDto>>), 200)]
        public async Task<IActionResult> GetRoles()
        {
            var response = await _roleService.GetRoles();
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint creates a role mapped to permissions
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CreateRole>), 400)]
        [ProducesResponseType(typeof(ApiResponse<CreateRole>), 200)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRole model)
        {
            var response = await _roleService.CreateRole(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint deletes a role
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 409)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        public async Task<IActionResult> DeleteRole([FromRoute] string id)
        {
            var response = await _roleService.DeleteRole(id);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint gets permissions for a role
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet("{roleId}/permissions")]
        [ProducesResponseType(typeof(ApiResponse<RolePermissionDto>), 404)]
        [ProducesResponseType(typeof(ApiResponse<RolePermissionDto>), 200)]
        public async Task<IActionResult> GetPermissionsForARole([FromRoute] string roleId)
        {
            var response = await _roleService.GetPermissionsForARole(roleId);
            return ParseResponse(response);
        }
    }
}
