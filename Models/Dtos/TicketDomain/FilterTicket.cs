using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Models.Dtos.TicketDomain
{
    public class FilterTicket : Pagination
    {
        public TicketStatus? TicketStatus { get; set; }
        public Guid? CategoryId { get; set; }
        public string? SearchKeyword { get; set; }
        public Priority? Priority { get; set; }
        public bool? IsRead { get; set; }
    }
}

