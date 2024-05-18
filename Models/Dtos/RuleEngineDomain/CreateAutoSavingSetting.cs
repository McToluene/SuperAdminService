namespace SuperAdmin.Service.Models.Dtos.RuleEngineDomain
{
    public class CreateAutoSavingSetting
    {
        public int DurationInMonths { get; set; }
        public double InterestPercentage { get; set; }
    }
}
