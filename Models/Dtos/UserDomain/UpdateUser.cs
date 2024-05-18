namespace SuperAdmin.Service.Models.Dtos.UserDomain
{
    public class UpdateUser
    {
        public string? PhoneNumber { get; set; }
        public string? RoleName { get; set; }
        public Guid? CountryId { get; set; }
    }
}
