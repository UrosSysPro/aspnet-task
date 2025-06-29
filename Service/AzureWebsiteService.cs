using Newtonsoft.Json;

namespace aspnet_task.Service;

using aspnet_task.Model;
using Newtonsoft.Json;
using aspnet_task.Utils;
using aspnet_task.Dto;

public class AzureWebsiteService
{
    public interface ITimeEntryResponse{}
    public class TimeEntrySuccess : ITimeEntryResponse
    {
        public TimeEntrySuccess(TimeEntry[] entries){this.entries=entries;}
        public TimeEntry[] entries { get; set; }
    }
    public class TimeEntryFailure : ITimeEntryResponse
    {
        public TimeEntryFailure(string message){ErrorMessage=message;}
        public string ErrorMessage { get; set; }
    }
    
    private static string apiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api";
    
    public static async Task<ITimeEntryResponse> GetTimeEntries(HttpClient http,string code)
    {
        static async Task<HttpResponseMessage> getResponse(HttpClient http, string code)
        {
            return await http.GetAsync($"{apiUrl}/gettimeentries?code={code}");
        }

        static async Task<TimeEntry[]> responseToTimeEntry(HttpResponseMessage response)
        {
            var json=await response.Content.ReadAsStringAsync();
            var dtoEntries = JsonConvert.DeserializeObject<TimeEntryDto[]>(json) ?? new TimeEntryDto[0];
            var entries=new  TimeEntry[dtoEntries.Length];
            for (int i = 0; i < dtoEntries.Length; i++) entries[i]=TimeEntryUtils.TimeEntryDtoToModel(dtoEntries[i]);
            return entries;
        }

        try
        {
            var response=await getResponse(http,code);
            var entries=await responseToTimeEntry(response);
            return new TimeEntrySuccess(entries);
        }
        catch (Exception e)
        {
            return new TimeEntryFailure(e.Message);
        }
    }
}
