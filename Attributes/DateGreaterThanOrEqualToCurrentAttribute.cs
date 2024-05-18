using System.ComponentModel.DataAnnotations;

namespace SuperAdmin.Service.Attributes
{
	public class DateGreaterThanOrEqualToCurrentAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            DateTime? dateValue = (DateTime?)value;
            if (dateValue != null && dateValue >= DateTime.Now)
                return ValidationResult.Success;

            string? propertyName = validationContext?.MemberName;
            return new ValidationResult($"{propertyName} must be greater than or equal to the current time.");
        }
    }
}

