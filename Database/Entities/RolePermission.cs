using System.ComponentModel.DataAnnotations.Schema;

namespace SuperAdmin.Service.Database.Entities
{
    public class RolePermission
    {
        public string RoleId { get; set; }
        
        public string PermissionId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; }
    }
}
