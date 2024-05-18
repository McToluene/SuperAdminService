namespace SuperAdmin.Service.Database.Entities
{
    public class AutoSavingSetting : BaseEntity
    {
        public AutoSavingSetting()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Description { get; set; }
        public int DurationInMonths { get; set; }
        public double InterestPercentage { get; set; }
    }
}
