namespace SuperAdmin.Service.Models.Dtos.SubscriptionDomain;

public class TenantSubscription
{
    public string CompanyName { get; set; }
    public string Country { get; set; }
    public string Email { get; set; }
    public string PlanName { get; set; }
    public int UsersInPlan { get; set; }
    public DateTime TransactionDate { get; set; }
    public double Amount { get; set; }
    public string PaymentProvider { get; set; }
    public string TenantId { get; set; }
}
