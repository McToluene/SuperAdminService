using SuperAdmin.Service.Attributes;
using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Models.Dtos.TicketDomain
{
    public class BulkTicketAssign
    {
        public List<Guid> Tickets { get; set; }
        public Priority Priority { get; set; }

        [DateGreaterThanOrEqualToCurrentAttribute]
        public DateTime DueDate { get; set; }
        public string UserToAssign { get; set; }
    }
}

