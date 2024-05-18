using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Models.Dtos.RuleEngineDomain
{
    public class UpdateSalaryDisbursementCharge
    {
        public double Percentage { get; set; }
        public ChargeDeductionPeriod ChargeDeductionPeriod { get; set; }
    }
}
