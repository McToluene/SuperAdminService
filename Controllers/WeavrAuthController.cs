using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Models.Dtos.WeavrDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeavrAuthController : BaseController
    {
        private readonly IWeavrClientService _weavrClientService;

        public WeavrAuthController(IWeavrClientService weavrClientService)
        {
            _weavrClientService = weavrClientService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginWithPassword(LoginRequest model)
        {
            var response = await _weavrClientService.LoginWithPassword(model);
            return ParseResponse(response);
        }
    }
}
