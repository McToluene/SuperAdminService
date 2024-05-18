namespace SuperAdmin.Service.Models.Dtos.WeavrDomain
{
    public class CorporateIdentityResponse
    {
        public Id id { get; set; }
        public string profileId { get; set; }
        public string tag { get; set; }
        public RootCorporateUser rootUser { get; set; }
        public Company company { get; set; }
        public string industry { get; set; }
        public string sourceOfFunds { get; set; }
        public string sourceOfFundsOther { get; set; }
        public bool acceptedTerms { get; set; }
        public string ipAddress { get; set; }
        public string baseCurrency { get; set; }
        public string feeGroup { get; set; }
        public long creationTimestamp { get; set; }
    }

    public class Id
    {
        public string type { get; set; }
        public string id { get; set; }
    }
}
