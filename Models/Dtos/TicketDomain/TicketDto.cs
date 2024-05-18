using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Models.Dtos.TicketDomain
{
    public class TicketDto : SuperAdminCreateTicket
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TicketActionLogDto?> TicketActionLogs { get; set; }
        public TicketStatus Status { get; set; }
        public DateTime DueDate { get; set; }
        public string ReferenceId { get; set; }
        public string? CategoryName { get; set; }
        public bool IsRead { get; set; }
        public List<TicketReplyDto> TicketReplies { get; set; }
    }
}
