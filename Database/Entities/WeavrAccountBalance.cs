using System.ComponentModel.DataAnnotations.Schema;

namespace SuperAdmin.Service.Database.Entities
{
    public class WeavrAccountBalance : BaseEntity
    {
        public WeavrAccountBalance()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string AccountId { get; set; }
        [Column(TypeName = "decimal(20,2)")]
        public decimal AvailableBalance { get; set; }
        [Column(TypeName = "decimal(20,2)")]
        public decimal ActualBalance { get; set; }

        [ForeignKey("AccountId")]
        public WeavrCorporateAccount WeavrCorporateAccount { get; set; }
    }
}
