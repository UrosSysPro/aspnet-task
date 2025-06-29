namespace aspnet_task.Model;

public class TimeEntryPerUser
{
    public TimeEntryPerUser(string EmployeeName, float TotalWorkingHours)
    {
        this.EmployeeName=EmployeeName;
        this.TotalWorkingHours=TotalWorkingHours;
    }
    public string EmployeeName { get; set; }
    public float TotalWorkingHours { get; set; }
}
