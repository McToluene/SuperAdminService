using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Database.Entities
{
    public class TicketReply : BaseEntity
    {
        public TicketReply()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Message { get; set; }
        public MessageType MessageType { get; set; }
        public string PostedByUserId { get; set; }
        public UserType UserType { get; set; }
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; }
    }
}