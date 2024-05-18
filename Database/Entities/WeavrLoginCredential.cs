using System.ComponentModel.DataAnnotations.Schema;

namespace SuperAdmin.Service.Database.Entities
{
    public class WeavrLoginCredential : BaseEntity
    {
        public WeavrLoginCredential()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
