using System.ComponentModel.DataAnnotations;
using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Models.Dtos.TicketDomain
{
    public class SuperAdminCreateTicket
    {
        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string CustomerEmail { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        public string AssignedUserId { get; set; }
        public Priority Priority { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }
    }
}