using System.ComponentModel.DataAnnotations;
using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Database.Entities
{
    public class Ticket : BaseEntity
    {
        public Ticket()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string ReferenceId { get; set; }
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string CustomerEmail { get; set; }
        public Priority Priority { get; set; }
        [Required]
        public Guid TicketCategoryId { get; set; }
        public TicketCategory TicketCategory { get; set; }
        public TicketStatus Status { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Message { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsRead { get; set; }
        public string? AssignedUserId { get; set; }
        public User AssignedUser { get; set; }
        public DateTime DueDate { get; set; }
        public ICollection<TicketActionLog> TicketActionLogs { get; } = new List<TicketActionLog>();
        public ICollection<TicketReply> TicketReplies { get; } = new List<TicketReply>();
    }
}

