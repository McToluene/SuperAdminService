using Microsoft.AspNetCore.Identity;

namespace SuperAdmin.Service.Database.Entities
{
    public class Role : IdentityRole
    {
        public Role()
        {
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = DateTime.UtcNow;
        }
        
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public virtual IEnumerable<Permission> Permissions { get; }
        public virtual IEnumerable<RolePermission> RolePermissions { get; }

    }
}
