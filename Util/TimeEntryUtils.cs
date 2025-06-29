namespace aspnet_task.Utils;

using aspnet_task.Model;
using aspnet_task.Dto;

public class TimeEntryUtils
{
    public static TimeEntry TimeEntryDtoToModel(TimeEntryDto dto)
    {
        return new TimeEntry(
            dto.Id,
            dto.EmployeeName,
            DateTime.Parse(dto.StarTimeUtc),
            DateTime.Parse(dto.EndTimeUtc),
            dto.EntryNotes,
            dto.DeletedOn == null ? null : DateTime.Parse(dto.DeletedOn)
        );
    }

    public static TimeEntryPerUser[] CompactTimeEntries(TimeEntry[] entries)
    {
        var dict=new Dictionary<string,TimeEntryPerUser>();
        foreach (var entry in entries)
        {
            if(entry.EmployeeName==null)continue;
            if (dict.ContainsKey(entry.EmployeeName))
            {
                var userEntry=dict[entry.EmployeeName];
                userEntry.TotalWorkingHours+=entry.TotalWorkingHours;
                dict[entry.EmployeeName] = userEntry;
            }
            else
            {
                dict[entry.EmployeeName] = TimeEntryToTimeEntryPerUser(entry);
            }
        }
        return dict.Values.ToArray().OrderByDescending(entry=>entry.TotalWorkingHours).ToArray();
    }
    
    public static TimeEntryPerUser TimeEntryToTimeEntryPerUser(TimeEntry dto)
    {
        return new TimeEntryPerUser(
            dto.EmployeeName,
            (float)dto.EndTimeUtc.Subtract(dto.StarTimeUtc).TotalHours
            );
    }
}
