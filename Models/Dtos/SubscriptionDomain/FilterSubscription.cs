using System.ComponentModel.DataAnnotations;
using Identification;
using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Models.Dtos.SubscriptionDomain;

public class FilterSubscription
{
    [Required]
    public Applications Application { get; set; }

    public PaymentProviders? Provider { get; set; }

    [Required]
    public TimePeriodFilter PeriodFilter { get; set; }
    [DateRangeValidation]
    public DateTime? FromDate { get; set; }
    [DateRangeValidation]
    public DateTime? ToDate { get; set; }
}
