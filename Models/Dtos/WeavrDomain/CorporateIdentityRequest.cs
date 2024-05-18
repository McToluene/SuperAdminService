namespace SuperAdmin.Service.Models.Dtos.WeavrDomain
{
    public class CorporateIdentityRequest
    {
        public RootCorporateUser rootUser { get; set; }
        public Company company { get; set; }
        public BaseCurrency baseCurrency { get; set; }
    }

    public class Company
    {
        /// <summary>
        /// Accepted values: "SOLE_TRADER", "LLC", "PUBLIC_LIMITED_COMPANY", "LIMITED_LIABILITY_PARTNERSHIP", "NON_PROFIT_ORGANISATION"
        /// </summary>
        public CompanyType type { get; set; }
        public BusinessAddress businessAddress { get; set; }
        public string name { get; set; }
        public string registrationNumber { get; set; }
        public string registrationCountry { get; set; }
    }

    public class RootCorporateUser
    {
        public string name { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public Mobile mobile { get; set; }
        /// <summary>
        /// Accepted Values: "DIRECTOR", "AUTHORISED_REPRESENTATIVE"
        /// </summary>
        public CompanyPosition companyPosition { get; set; }
        public DateOfBirth dateOfBirth { get; set; }
    }

    public class BusinessAddress
    {
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string city { get; set; }
        public string postCode { get; set; }
        public string state { get; set; }
        public string country { get; set; }
    }

    public class DateOfBirth
    {
        public int year { get; set; }
        public int month { get; set; }
        public int day { get; set; }
    }

    public class Mobile
    {
        public string countryCode { get; set; }
        public string number { get; set; }
    }

    public enum CompanyPosition
    {
        DIRECTOR = 1,
        AUTHORISED_REPRESENTATIVE
    }

    public enum CompanyType
    {
        SOLE_TRADER = 1,
        LLC, 
        PUBLIC_LIMITED_COMPANY, 
        LIMITED_LIABILITY_PARTNERSHIP,
        NON_PROFIT_ORGANISATION
    }

    public enum BaseCurrency
    {
        USD = 1,
        EUR,
        GBP
    }

    public class CorporateIdentity
    {
        public string profileId { get; set; }
        public RootCorporateUser rootUser { get; set; }
        public Company company { get; set; }
        public bool acceptedTerms { get; set; }
        public string ipAddress { get; set; }
        public string baseCurrency { get; set; }
    }
}
