using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperAdmin.Service.Database.Entities
{
    public class WeavrCorporateUser : BaseEntity
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsPhoneNumberVerified { get; set; }
        [Range(1, 31, ErrorMessage = "Value should be within 1 - 31")]
        public int DayOfBirth { get; set; }
        [Range(1, 12, ErrorMessage = "Value should be within 1 - 12")]
        public int MonthOfBirth { get; set; }
        public int YearOfBirth { get; set; }
        public string BaseCurrency {  get; set; }
        public bool IsOnboardingCompleted { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
