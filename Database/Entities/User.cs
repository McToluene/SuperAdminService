using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperAdmin.Service.Database.Entities
{
    public class User : IdentityUser
    {
        public User()
        {
            CreatedAt = DateTime.Now;
            ModifiedAt = DateTime.Now;
            PasswordExpiryDate = DateTime.Now.AddMonths(1);
        }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string? PinHash { get; set; }
        public bool IsPinSet { get; set; }
        public Guid? CountryId { get; set; }
        [ForeignKey("CountryId")]
        public virtual Country Country { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool IsSuspended { get; set; }
        public bool IsDeleted { get; set; }
        public bool HasPasswordChanged {  get; set; }
        public DateTime PasswordExpiryDate {  get; set; }
    }
}
