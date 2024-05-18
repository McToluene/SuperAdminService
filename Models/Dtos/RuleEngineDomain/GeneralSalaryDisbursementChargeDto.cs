using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Models.Dtos.RuleEngineDomain
{
    public class GeneralSalaryDisbursementChargeDto
    {
        public ChargeDeductionPeriod ChargeDeductionPeriod { get; set; }
        public double Percentage { get; set; }
    }
}
