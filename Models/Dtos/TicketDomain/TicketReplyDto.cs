using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Models.Dtos.TicketDomain
{
    public class TicketReplyDto
    {
        public MessageType MessageType { get; set; }

        public string? Message { get; set; }
    }
}