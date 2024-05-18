namespace SuperAdmin.Service.Models.Enums
{
    /// <summary>
    /// Classifies salary disbursement charge setting under general or custom
    /// </summary>
    public enum SalaryDisbursementSettingType
    {
        General = 1,
        Custom
    }

    /// <summary>
    /// To denote whether a disbursement charge setting is default or modified
    /// </summary>
    public enum SalaryDisbursementSettingStatus
    {
        Default = 1,
        Modified
    }

    /// <summary>
    /// Period to bill the company before salary is disbursed
    /// </summary>
    public enum ChargeDeductionPeriod
    {
        OnPaymentDay = 1,
        Monthly,
        Bimonthly,
        Quarterly,
        Biannually,
        Yearly
    }
}
