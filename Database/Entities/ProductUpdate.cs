using SuperAdmin.Service.Database.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperAdmin.Service.Database.Entities
{
    public class ProductUpdate : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Package {  get; set; }
        public string Subject { get; set; }
        public string Body {  get; set; }
        public string ImageUrl { get; set; }
        public PublishStatus PublishStatus {  get; set; }

        [ForeignKey("CategoryId")]
        public virtual ProductUpdateCategory ProductUpdateCategory { get; set; }
    }
}
