using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos.WeavrDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class WeavrAccountController : BaseController
    {
        private readonly IWeavrClientService _weavrClientService;

        public WeavrAccountController(IWeavrClientService weavrClientService)
        {
            _weavrClientService = weavrClientService;    
        }

        [HttpPost("managed-accounts")]
        public async Task<IActionResult> CreateManagedAccount([FromHeader] string token, [FromBody] CreateManagedAccount model)
        {
            var response = await _weavrClientService.CreateManagedAccount(model, token);
            return ParseResponse(response);
        }
    }
}
