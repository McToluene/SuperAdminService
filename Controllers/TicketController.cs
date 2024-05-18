using System.Security.Claims;
using Configurations.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SuperAdmin.Service.Database.Enums;
using SuperAdmin.Service.Helpers;
using SuperAdmin.Service.Models.Dtos;
using SuperAdmin.Service.Models.Dtos.TicketDomain;
using SuperAdmin.Service.Services.Contracts;

namespace SuperAdmin.Service.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TicketController : BaseController
    {
        private readonly ITicketService _ticketService;
        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        /// <summary>
        /// This endpoint creates a ticket
        /// <param name="superAdminCreateTicket"></param>
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 404)]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateTicket([FromBody] SuperAdminCreateTicket superAdminCreateTicket)
        {
            var response = await _ticketService.CreateTicket(superAdminCreateTicket);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint creates a ticket
        /// <param name="adminCreateTicket"></param>
        /// </summary>
        /// <returns></returns>
        [HttpPost("admin")]
        [Authorize(AuthenticationSchemes = "monolith-schema")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 404)]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
        public async Task<IActionResult> CreateTicket([FromBody] AdminCreateTicket adminCreateTicket)
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
                return Unauthorized();

            var response = await _ticketService.CreateTicket(adminCreateTicket, name, email);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint gets all ticket
        /// </summary>
        /// <param name="sortTicket"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginationResult<TicketDto>>), 200)]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetTickets([FromQuery] SortTicket sortTicket)
        {
            var response = await _ticketService.GetTickets(sortTicket);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint gets all ticket
        /// </summary>
        /// <param name="sortTicket"></param>
        /// <returns></returns>
        [HttpGet("admin")]
        [ProducesResponseType(typeof(ApiResponse<PaginationResult<TicketDto>>), 200)]
        [Authorize(AuthenticationSchemes = "monolith-schema")]
        public async Task<IActionResult> GetTicketsForAdmin([FromQuery] SortTicket sortTicket)
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var response = await _ticketService.GetTickets(sortTicket, email);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint get tickets statistics for assigned users
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPut("bulk")]
        [ProducesResponseType(typeof(ApiResponse<List<TicketDto?>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<List<TicketDto?>>), 400)]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> BulkAssign([FromBody] BulkTicketAssign bulkTicketAssign)
        {
            string? id = GetIdFromToken();
            if (string.IsNullOrEmpty(id))
                return Unauthorized("Token not valid!");

            var response = await _ticketService.BulkAssign(bulkTicketAssign, id);
            return ParseResponse(response);
        }


        /// <summary>
        /// This endpoint to get a ticket
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        [HttpGet("{ticketId}")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 404)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + "monolith-schema")]
        public async Task<IActionResult> GetTicket([FromRoute] Guid ticketId)
        {
            var isRead = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value == HelperConstants.SUPER_ADMIN_ROLE;
            var response = await _ticketService.GetTicket(ticketId, isRead);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint to delete a ticket
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        [HttpDelete("{ticketId}")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 404)]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteTicket([FromRoute] Guid ticketId)
        {
            var response = await _ticketService.DeleteTicket(ticketId);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint to delete a ticket for admin users
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        [HttpDelete("admin/{ticketId}")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 404)]
        [Authorize(AuthenticationSchemes = "monolith-schema")]
        public async Task<IActionResult> DeleteTicketForAdmin([FromRoute] Guid ticketId)
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var response = await _ticketService.DeleteTicket(ticketId, email);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint creates a ticket
        /// </summary>
        /// <returns></returns>
        [HttpPut("{ticketId}/assign-actions")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 404)]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PerformAllAction([FromRoute] Guid ticketId, [FromBody] TicketCommand command)
        {
            string? id = GetIdFromToken();
            if (string.IsNullOrEmpty(id))
                return Unauthorized("Token not valid!");

            var response = await _ticketService.PerformAllAction(ticketId, command, id);
            return ParseResponse(response);
        }


        /// <summary>
        /// This endpoint to assign a user ticket
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPatch("{ticketId}/assign/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 409)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> AssignUser([FromRoute] Guid ticketId, [FromRoute] string userId)
        {
            string? id = GetIdFromToken();
            if (string.IsNullOrEmpty(id))
                return Unauthorized("Token not valid!");

            var response = await _ticketService.AssignUser(ticketId, userId, id);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint change ticket status
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPatch("{ticketId}/status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 409)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ChangeStatus([FromRoute] Guid ticketId, [FromRoute] TicketStatus status)
        {
            string? id = GetIdFromToken();
            if (string.IsNullOrEmpty(id))
                return Unauthorized("Token not valid!");

            var response = await _ticketService.ChangeStatus(ticketId, status, id);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint set ticket priority
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        [HttpPatch("{ticketId}/priority/{priority}")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 409)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SetPriority([FromRoute] Guid ticketId, [FromRoute] Priority priority)
        {
            string? id = GetIdFromToken();
            if (string.IsNullOrEmpty(id))
                return Unauthorized("Token not valid!");

            var response = await _ticketService.SetPriority(ticketId, priority, id);
            return ParseResponse(response);
        }


        /// <summary>
        /// This endpoint set ticket priority
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        [HttpPatch("{ticketId}/due/{dueDate}")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 404)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 409)]
        [ProducesResponseType(typeof(ApiResponse<dynamic>), 400)]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SetDueDate([FromRoute] Guid ticketId, [FromRoute] DateTime dueDate)
        {
            string? id = GetIdFromToken();
            if (string.IsNullOrEmpty(id))
                return Unauthorized("Token not valid!");

            var response = await _ticketService.SetDueDate(ticketId, dueDate, id);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint get tickets statistics
        /// </summary>
        /// <returns></returns>
        [HttpGet("statistics")]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(ApiResponse<TicketStatistics>), 200)]
        public async Task<IActionResult> GetTicketStatistics()
        {
            var response = await _ticketService.GetTicketStatistics(string.Empty);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint get tickets statistics for assigned users
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("statistics/{userId}")]
        [Authorize(Roles = HelperConstants.SUPER_ADMIN_ROLE, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(ApiResponse<TicketStatistics>), 200)]
        public async Task<IActionResult> GetTicketStatistics([FromRoute] string userId)
        {
            var response = await _ticketService.GetTicketStatistics(userId);
            return ParseResponse(response);
        }

        /// <summary>
        /// This endpoint get tickets statistics for assigned users
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="replyTicket"></param>
        /// <returns></returns>
        [HttpPost("{ticketId}/reply")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + "monolith-schema")]
        [ProducesResponseType(typeof(ApiResponse<List<ReplyDto>>), 200)]
        public async Task<IActionResult> Reply([FromRoute] Guid ticketId, [FromForm] ReplyTicket replyTicket)
        {
            string? id = string.Empty;
            if (replyTicket.ReplyFrom.Equals(UserType.SUPER_ADMIN))
                id = GetIdFromToken();
            else if (replyTicket.ReplyFrom.Equals(UserType.ADMIN))
                id = GetIdFromTokenAdmin();

            if (string.IsNullOrEmpty(id))
                return Unauthorized("Token not valid!");

            var response = await _ticketService.ReplyTicket(ticketId, replyTicket, id);
            return ParseResponse(response);
        }

        private string? GetIdFromToken()
        {
            var identityOptions = new IdentityOptions();
            ClaimsPrincipal user = User;
            Claim? id = user.FindFirst(identityOptions.ClaimsIdentity.UserIdClaimType);
            return id?.Value;
        }

        private string? GetIdFromTokenAdmin()
        {
            var identifiers = User.FindAll(c => c.Type.Equals(ClaimTypes.NameIdentifier));
            foreach (Claim claim in identifiers)
            {
                if (Guid.TryParse(claim.Value, out Guid guid))
                    return guid.ToString();
            }
            return null;
        }
    }
}

