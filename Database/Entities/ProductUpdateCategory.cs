namespace SuperAdmin.Service.Database.Entities
{
    public class ProductUpdateCategory : BaseEntity
    {
        public ProductUpdateCategory()
        {
            Id = Guid.NewGuid();
            IsDeleted = false;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
    }
}
