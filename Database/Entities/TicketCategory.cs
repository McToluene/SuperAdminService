using System.ComponentModel.DataAnnotations;

namespace SuperAdmin.Service.Database.Entities
{
	public class TicketCategory: BaseEntity
	{
		public TicketCategory()
		{
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public bool IsDeleted { get; set; }
    }
}

