using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Database.Entities
{
    public class SalaryDisbursementChargeSetting : BaseEntity
    {
        public SalaryDisbursementChargeSetting()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public double Percentage { get; set; }
        public SalaryDisbursementSettingType SettingType { get; set; }
        public ChargeDeductionPeriod ChargeDeductionPeriod { get; set; }
    }
}
