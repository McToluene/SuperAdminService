using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Database.Entities
{
    public class Permission : BaseEntity
    {
        public Permission()
        {
            Id = Guid.NewGuid().ToString();
        }
        public string Id { get; set; }
        public string Description { get; set; }
        public PermissionType Type { get; set; }
        public PermissionCategory Category { get; set; }
        public virtual IEnumerable<Role> Roles { get; }
        public virtual IEnumerable<RolePermission> RolePermissions { get; set; }
    }
}
