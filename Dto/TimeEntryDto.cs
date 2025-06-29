namespace aspnet_task.Dto;

public class TimeEntryDto
{
    public string Id { get; set; }    
    public string? EmployeeName { get; set; }
    public string StarTimeUtc { get; set; }
    public string EndTimeUtc { get; set; }
    public string EntryNotes { get; set; }
    public string? DeletedOn { get; set; }
}
