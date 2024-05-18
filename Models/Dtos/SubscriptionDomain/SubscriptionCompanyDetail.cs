namespace SuperAdmin.Service.Models.Dtos.SubscriptionDomain;

public class SubscriptionCompanyDetail
{
    public string CompanyName { get; set; }
    public string Country { get; set; }
    public string Email { get; set; }
    public string PlanName { get; set; }
    public int StaffSize { get; set; }
    public DateTime RegistrationDate { get; set; }
}
