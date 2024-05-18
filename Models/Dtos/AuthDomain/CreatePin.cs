namespace SuperAdmin.Service.Models.Dtos.AuthDomain
{
    public class CreatePin
    {
        public string UserId { get; set; }
        public string Pin { get; set; }
    }

    public class ValidatePin : CreatePin { }
}
