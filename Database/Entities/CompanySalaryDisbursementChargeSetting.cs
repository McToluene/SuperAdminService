using SuperAdmin.Service.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperAdmin.Service.Database.Entities
{
    public class CompanySalaryDisbursementChargeSetting : BaseEntity
    {
        public CompanySalaryDisbursementChargeSetting()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        /// <summary>
        /// The company id gotten from Pays record
        /// </summary>
        public string PaysCompanyId { get; set; }
        public string CompanyName { get; set; }
        public SalaryDisbursementSettingStatus SettingStatus { get; set; }
        public DateTime SalaryPaymentDate { get; set; }
        public DateTime NextChargeDate { get; set; }
        public bool IsNewRecord { get; set; }
        public Guid SalaryDisbursementChargeSettingId { get; set; }
        [ForeignKey("SalaryDisbursementChargeSettingId")]
        public virtual SalaryDisbursementChargeSetting SalaryDisbursementChargeSetting { get; set; }
    }
}
