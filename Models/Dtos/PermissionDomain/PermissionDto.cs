using SuperAdmin.Service.Models.Enums;

namespace SuperAdmin.Service.Models.Dtos.PermissionDomain
{
    public class PermissionDto
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public PermissionType Type { get; set; }
        public PermissionCategory Category { get; set; } 
        public double CreatedAt { get; set; }
    }
}
