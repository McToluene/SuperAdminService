namespace SuperAdmin.Service.Models.Dtos.RuleEngineDomain
{
    public class UpdateAutoSavingSettingDto
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Pin {  get; set; }
        public string Otp {  get; set; }
        public UpdateAutoSavingSetting[] AutoSavingSettings { get; set; }
    }

    public class UpdateAutoSavingSetting
    {
        public Guid AutoSavingSettingId { get; set; }
        public double InterestPercentage { get; set; } 
    }
}
