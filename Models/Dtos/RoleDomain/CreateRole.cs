namespace SuperAdmin.Service.Models.Dtos.RoleDomain
{
    public class CreateRole
    {
        public string RoleName { get; set; }
        public string[] PermissionIds { get; set; }
    }
}
