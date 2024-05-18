namespace SuperAdmin.Service.Models.Dtos.ProjectDomain;
public class CompanyDetailDto
{
    public string CompanyName { get; set; }
    public int ProjectCount { get; set; }
    public DateTime? CreationDate { get; set; }
    public int StaffSize { get; set; }
    public int TotalProjectCount { get; set; }
}