using System.ComponentModel.DataAnnotations;
using SuperAdmin.Service.Database.Enums;

namespace SuperAdmin.Service.Attributes
{
    public class MessageTypeValidationAttribute : ValidationAttribute
    {
        private readonly MessageType[] _allowedMessageTypes;

        public MessageTypeValidationAttribute(params MessageType[] allowedMessageTypes)
        {
            _allowedMessageTypes = allowedMessageTypes;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var messageTypeProperty = validationContext.ObjectType.GetProperty("MessageType");
            if (messageTypeProperty != null && validationContext.ObjectInstance != null)
            {
                var messageTypeValue = messageTypeProperty.GetValue(validationContext.ObjectInstance) as MessageType?;
                if (messageTypeValue.HasValue && _allowedMessageTypes.Contains(messageTypeValue.Value))
                    return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage ?? "Invalid combination of MessageType and property requirements.");
        }
    }
}