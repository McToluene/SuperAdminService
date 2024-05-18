using System.ComponentModel.DataAnnotations;
using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Models.Dtos.ProjectDomain;

public class FilterProject : Pagination
{
    [Required]
    public Applications Application { get; set; }
    [Required]
    public CompanyProjectFilter Filter { get; set; }

}