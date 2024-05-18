using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Database.Entities
{
    public class TicketActionLog : BaseEntity
    {
        public TicketActionLog()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public ActionType ActionType { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Message { get; set; }
        public string PerformedByUserId { get; set; }
        public User PerformedByUser { get; set; }
        public Guid TicketId { get; set; }
        public Ticket Ticket { get; set; }
    }
}