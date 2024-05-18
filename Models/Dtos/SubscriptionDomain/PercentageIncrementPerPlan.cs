namespace SuperAdmin.Service.Models.Dtos.SubscriptionDomain;

public class PercentageIncrementPerPlanDto
{
    public Guid PricingPlanId { get; set; }
    public string Name { get; set; }
    public double PercentageIncrement { get; set; }
    public int SubscriptionCount { get; set; }
    public double Revenue { get; set; }
}