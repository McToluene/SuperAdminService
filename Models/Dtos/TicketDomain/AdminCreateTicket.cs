using System.ComponentModel.DataAnnotations;

namespace SuperAdmin.Service.Models.Dtos.TicketDomain;

public class AdminCreateTicket
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public string Subject { get; set; }

    [Required]
    public string Message { get; set; }
}
