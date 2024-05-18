namespace SuperAdmin.Service.Models.Dtos.WeavrDomain
{
    public class ManagedAccountResponse
    {
        public string id { get; set; }
        public string profileId { get; set; }
        public string tag { get; set; }
        public string friendlyName { get; set; }
        public string currency { get; set; }
        public Balances balances { get; set; }
        public State state { get; set; }
        public long creationTimestamp { get; set; }
    }

    public class Balances
    {
        public decimal availableBalance { get; set; }
        public decimal actualBalance { get; set; }
    }

    public class State 
    {
        public string state { get; set; }
        public string blockedReason { get; set; }
        public string destroyedReason { get; set; }
    }
}
