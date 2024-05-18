using System.ComponentModel.DataAnnotations.Schema;

namespace SuperAdmin.Service.Database.Entities
{
    public class PayrollVendorChargeSeting : BaseEntity
    {
        public Guid Id { get; set; }
        public double Percentage { get; set; }
        public Guid ChargeDeductionBasisId { get; set; }
        [ForeignKey("ChargeDeductionBasisId")]
        public PayrollVendorChargeDeductionBasis PayrollVendorChargeDeductionBasis { get; set; }
    }
}
