namespace SuperAdmin.Service.Models.Dtos.SubscriptionDomain;

public class ApplicationStatistic
{
    public int? TotalSubscriptions { get; set; }
    public int? TotalCompanies { get; set; }
    public double? TotalRevenue { get; set; }
    public int? PendingRequest { get; set; }
    public int? TotalActive { get; set; }
}