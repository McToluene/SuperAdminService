using System.ComponentModel.DataAnnotations;
using Identification;

namespace SuperAdmin.Service;

public class DateRangeValidation : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var fromDateProperty = validationContext.ObjectType.GetProperty("FromDate");
        var toDateProperty = validationContext.ObjectType.GetProperty("ToDate");
        var periodFilterProperty = validationContext.ObjectType.GetProperty("PeriodFilter");

        if (fromDateProperty != null && toDateProperty != null && periodFilterProperty != null)
        {
            var fromDateValue = (DateTime?)fromDateProperty.GetValue(validationContext.ObjectInstance, null);
            var toDateValue = (DateTime?)toDateProperty.GetValue(validationContext.ObjectInstance, null);
            var periodFilterValue = (TimePeriodFilter?)periodFilterProperty.GetValue(validationContext.ObjectInstance, null);

            if (periodFilterValue == TimePeriodFilter.Custom && (!fromDateValue.HasValue || !toDateValue.HasValue))
            {
                return new ValidationResult("FromDate and ToDate are required when PeriodFilter is Custom.");
            }
        }
        return ValidationResult.Success;
    }
}
