namespace SuperAdmin.Service.Models.Dtos.RuleEngineDomain
{
    public class UpdatePayrollVendorChargeDto
    {
        public Guid ChargeDeductionBasisId { get; set; }
        public double Percentage { get; set; }
    }
}
