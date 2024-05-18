namespace SuperAdmin.Service.Models.Dtos.WeavrDomain
{
    public class CreateManagedAccount
    {
        /// <summary>
        /// A unique identifier of the user can be used
        /// </summary>
        public string FriendlyName { get; set; }
        /// <summary>
        /// The currency expressed in ISO-4217 code.Example: GBP, EUR, USD.
        /// </summary>
        public string Currency { get; set; }
    }
}
