namespace SuperAdmin.Service.Models.Enums
{
    public enum OtpType
    {
        Login = 1,
        PinReset,
        AutoSavingSetting
    }

    public enum OtpStatus
    {
        Generated = 1,
        Used,
        Expired
    }
}
