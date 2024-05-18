namespace SuperAdmin.Service.Models.Dtos.UserDomain
{
    public class CreateUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string RoleName { get; set; }
        public string Email { get; set; }
        public Guid CountryId { get; set; }
    }
}
