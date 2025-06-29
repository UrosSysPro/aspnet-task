namespace aspnet_task.Model;

public class TimeEntry
{
    public TimeEntry(
        string id,
        string employeeName,
        DateTime starTimeUtc,
        DateTime endTimeUtc,
        string entryNotes,
        DateTime? deletedOn
    )
    {
        Id = id;
        EmployeeName = employeeName;
        StarTimeUtc = starTimeUtc;
        EndTimeUtc = endTimeUtc;
        EntryNotes = entryNotes;
        DeletedOn = deletedOn;
        TotalWorkingHours = CalculateTotalWorkingHours();
    }
    public string Id { get; set; }
    public string EmployeeName { get; set; }
    public DateTime StarTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public string EntryNotes { get; set; }
    public DateTime? DeletedOn { get; set; }
    public float TotalWorkingHours { get; set; }

    private float CalculateTotalWorkingHours()
    {
        return (float)EndTimeUtc.Subtract(StarTimeUtc).TotalHours;
    }
}

