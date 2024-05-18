using System.ComponentModel.DataAnnotations;
using SuperAdmin.Service.Attributes;
using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Models.Dtos.TicketDomain
{
    public class TicketCommand
    {
        [Required]
        public TicketStatus TicketStatus { get; set; }

        [Required]
        public string AssignUserId { get; set; }

        [Required]
        [DateGreaterThanOrEqualToCurrentAttribute]
        public DateTime DueDate { get; set; }

        [Required]
        public Priority Priority { get; set; }
    }
}

