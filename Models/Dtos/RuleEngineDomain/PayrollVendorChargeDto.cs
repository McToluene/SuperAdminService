namespace SuperAdmin.Service.Models.Dtos.RuleEngineDomain
{
    public class PayrollVendorChargeDto
    {
        public Guid Id { get; set; }
        public string ChargeDeductionBasis { get; set; }
        public double PercentageCharge { get; set; }
    }
}
