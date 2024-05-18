using System.ComponentModel.DataAnnotations;
using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Models.Dtos.TicketDomain
{
    public class ReplyTicket
    {
        [Required]
        public MessageType MessageType { get; set; }
        public string? Message { get; set; }
        public IFormFile? File { get; set; }
        [Required]
        public UserType ReplyFrom { get; set; }
    }
}