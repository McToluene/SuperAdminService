using SuperAdmin.Service.Models.Dtos.TicketDomain;
using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service;

public class SortTicket : FilterTicket
{
    public OrderBy? OrderBy { get; set; }
    public TicketSort? SortBy { get; set; }
}
