namespace SuperAdmin.Service.Models.Enums
{
    public enum PermissionType
    {
        Read = 1,
        Write 
    }

    public enum PermissionCategory
    {
        Dashboard = 1,
        Subscription,
        Wallet,
        ActivityLog,
        UserManagement,
        RoleManagement,
        Request,
        Settings,
        Notifications,
        RuleEngine
    }
}
