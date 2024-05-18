using Configurations.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Services.Contracts;



namespace SuperAdmin.Service.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TenantController : BaseController
    {
        private readonly ITenantService _tenantService;
        public TenantController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        /// <summary>
        /// This endpoint gets all tenants
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(ApiResponse<List<GetTenantResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        public async Task<IActionResult> GetTenants()
        {
            var response = await _tenantService.GetTenants();
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint gets a tenant
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetTenantResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        public async Task<IActionResult> GetTenant([FromRoute] string id)
        {
            var response = await _tenantService.GetTenant(id);
            return ParseResponse(response);
        }
    }
}
