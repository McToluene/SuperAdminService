using SuperAdmin.Service.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperAdmin.Service.Database.Entities
{
    public class OneTimePassword : BaseEntity
    {
        public OneTimePassword()
        {
            Id = Guid.NewGuid();
            OtpExpiryDate = DateTime.UtcNow.AddMinutes(5);
        }

        public Guid Id { get; set; }
        public string UserId { get; set; }
        public OtpType OtpType { get; set; }
        public OtpStatus OtpStatus { get; set; }
        public DateTime OtpExpiryDate { get; set; }
        public string Code { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmailAddress { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
