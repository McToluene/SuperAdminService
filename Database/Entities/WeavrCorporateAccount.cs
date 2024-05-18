namespace SuperAdmin.Service.Database.Entities
{
    public class WeavrCorporateAccount : BaseEntity
    {
        public string Id { get; set; }
        public string Currency {  get; set; }
        public string FriendlyName { get; set; }
        public string ProfileId { get; set; }
        public string State { get; set; }
    }
}
