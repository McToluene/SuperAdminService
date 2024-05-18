namespace SuperAdmin.Service.Models.Dtos.SubscriptionDomain;

public class TenantSubscriptionDetail : TenantSubscription
{
    public DateTime? ActivatedOn { get; set; }
    public DateTime? ExpiresOn { get; set; }
    public string Status { get; set; }
}
