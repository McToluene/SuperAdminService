namespace SuperAdmin.Service.Database.Entities
{
    public class Country : BaseEntity
    {
        public Country()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TwoLetterCode { get; set; }
        public string ThreeLetterCode { get; set; }
    }
}
