using Configurations.Utility;
using SuperAdmin.Service.Database.Enums;
using SuperAdmin.Service.Models.Dtos;
using SuperAdmin.Service.Models.Dtos.TicketDomain;

namespace SuperAdmin.Service.Services.Contracts
{
    public interface ITicketService
    {
        Task<ApiResponse<TicketDto>> CreateTicket(SuperAdminCreateTicket superAdminCreateTicket);
        Task<ApiResponse<TicketDto>> CreateTicket(AdminCreateTicket adminCreateTicket, string name, string email);
        Task<ApiResponse<TicketDto>> GetTicket(Guid ticketId, bool isRead);
        Task<ApiResponse<TicketDto>> DeleteTicket(Guid ticketId);
        Task<ApiResponse<TicketDto>> DeleteTicket(Guid ticketId, string? email);
        Task<ApiResponse<PaginationResult<TicketDto>>> GetTickets(SortTicket filter);
        Task<ApiResponse<PaginationResult<TicketDto>>> GetTickets(SortTicket filter, string? email);
        Task<ApiResponse<TicketDto>> PerformAllAction(Guid ticketId, TicketCommand command, string performedBy);
        Task<ApiResponse<TicketDto>> AssignUser(Guid ticketId, string userToAssign, string performedBy);
        Task<ApiResponse<TicketDto>> ChangeStatus(Guid ticketId, TicketStatus status, string performedBy);
        Task<ApiResponse<TicketDto>> SetPriority(Guid ticketId, Priority priority, string performedBy);
        Task<ApiResponse<TicketDto>> SetDueDate(Guid ticketId, DateTime dateTime, string performedBy);
        Task<ApiResponse<TicketStatistics>> GetTicketStatistics(string assignedUserId);
        Task<ApiResponse<List<TicketDto>>> BulkAssign(BulkTicketAssign bulkTicket, string performedBy);
        Task<ApiResponse<List<ReplyDto>>> ReplyTicket(Guid ticketId, ReplyTicket replyTicket, string performedBy);
        Task<ApiResponse<List<ReplyDto>>> GetReplies(Guid ticketId);
        Task<int> TicketCountByCategory(string category, TicketStatus status);
    }
}