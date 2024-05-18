using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Models.Dtos.TicketDomain
{
    public class TicketActionLogDto
    {
        public ActionType ActionType { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string PerformedByUserId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

