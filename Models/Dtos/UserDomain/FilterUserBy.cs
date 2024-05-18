using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Models.Dtos.UserDomain
{
    public class FilterUserBy : Pagination
    {
        public OrderBy? OrderBy { get; set; }
        public string? RoleName { get; set; }
    }
}
