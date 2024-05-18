using Configurations.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos.TicketCategoryDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Controllers
{

    [Route("api/v1/[controller]")]
    [ApiController]
    public class TicketCategoryController : BaseController
    {
        private readonly ITicketCategoryService _ticketCategoryService;
        public TicketCategoryController(ITicketCategoryService ticketCategoryService)
        {
            _ticketCategoryService = ticketCategoryService;
        }

        /// <summary>
        /// This endpoint gets all ticket categoreis
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<TicketCategoryDto>>), 200)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + "monolith-schema")]
        public async Task<IActionResult> GetTicketCategories()
        {
            var response = await _ticketCategoryService.GetCategories();
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint creates a ticket category
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TicketCategoryDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<TicketCategoryDto>), 200)]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateTicketCategory([FromBody] CreateTicketCatgeory model)
        {
            var response = await _ticketCategoryService.CreateCategory(model);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint updates a ticket category
        /// </summary>
        /// <param name="model"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<TicketCategoryDto>), 404)]
        [ProducesResponseType(typeof(ApiResponse<TicketCategoryDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<TicketCategoryDto>), 200)]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateTicketCategory([FromBody] CreateTicketCatgeory model, [FromRoute] string id)
        {
            var response = await _ticketCategoryService.UpdateCategory(model, id);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint deletes a ticket category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 200)]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteTicketCategory([FromRoute] string id)
        {
            var response = await _ticketCategoryService.DeleteTicketCategory(id);
            return ParseResponse(response);
        }
    }
}

