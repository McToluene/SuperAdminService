using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Models.Dtos.TicketDomain
{
    public class ReplyDto
    {
        public MessageType MessageType { get; set; }

        public string? Message { get; set; }

        public UserType UserType { get; set; }

        public string PostedByUserId { get; set; }
    }
}