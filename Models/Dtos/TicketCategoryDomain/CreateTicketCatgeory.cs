using System.ComponentModel.DataAnnotations;

namespace SuperAdmin.Service.Models.Dtos.TicketCategoryDomain
{
	public class CreateTicketCatgeory
	{
        [Required]
		public string CategoryName { get; set; }
	}
}
