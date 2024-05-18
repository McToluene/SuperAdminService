namespace SuperAdmin.Service.Database.Entities
{
    public class PayrollVendorChargeDeductionBasis : BaseEntity
    {
        public PayrollVendorChargeDeductionBasis()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
