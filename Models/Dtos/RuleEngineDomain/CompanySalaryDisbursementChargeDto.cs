using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Models.Dtos.RuleEngineDomain
{
    public class CompanySalaryDisbursementChargeDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; }
        public double SalaryPaymentDateInEpochMilliseconds { get; set; }
        public double NextChargeDateInEpochMilliseconds { get; set; }
        public double ChargePercentage {  get; set; }
        public SalaryDisbursementSettingStatus Status { get; set; }
        public ChargeDeductionPeriod ChargeDeductionPeriod { get; set; }
        public bool IsNewRecord { get; set; }
    }
}
