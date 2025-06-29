using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Drawing.Processing;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();
app.MapGet("/", TimeEntriesPage);
app.MapGet("/image", TimeEntriesPieChart);


async static Task<IResult> TimeEntriesPage(HttpClient httpClient)
{
    var code = "vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
    var response = await AzureWebsiteService.GetTimeEntries(httpClient, code);

    static string PageSuccess(TimeEntryPerUser[] entries)
    {
        var tbody = new StringBuilder();
        tbody.AppendLine("<tbody>");
        foreach (var entry in entries)
        {
            var className = entry.TotalWorkingHours < 100 ? " class='lowerThen'" : "";
            var name = entry.EmployeeName;
            var workingHours = (int)Math.Round(entry.TotalWorkingHours);
            tbody.AppendLine($@"
            <tr{className}>
                <td class=""align-left p-1"">{name}</td>
                <td class=""align-center p-1"">{workingHours} hrs</td>
                <td class=""align-center p-1""><a href='#'>Edit</a></td>
            </tr>
        ");
        }
        tbody.AppendLine("</tbody>");
        var html = $@"
            <!DOCTYPE html> 
            <html>
             <head>
                <style>
                    *{{
                        padding: 0px;
                        margin: 0px;
                        box-sizing: border-box;
                        font-family: sans-serif;
                    }}
                    html{{
                        height: 100vh;
                    }}
                    #content{{
                        display:flex;
                        flex-direction: column;
                        align-items: center;
                        justify-content: center;
                        overflow-x: hidden;
                        overflow-y: auto;
                    }}
                    body{{
                        height: 100%;
                        display:flex;
                        flex-direction: column;
                        align-items: center;
                        justify-content: center;
                    }}
                    #scrollable{{
                        display: block;
                        width: 40rem;
                        height: 20rem;
                        overflow-x: hidden;
                        overflow-y: auto;
                      }}
                      
                      #table-gray-border-collapse th,td{{
                        border:2px gray solid;
                      }}
                      #table-gray-border-collapse {{
                        /*border:2px gray solid;*/
                        border-collapse: collapse;
                      }}
                      
                      .align-left{{
                        text-align: start;
                      }}
                      .align-center{{
                        text-align: center;
                      }}
                      .w-full{{
                        width: 100%;
                      }}
                      .px-1{{
                        padding-left: 1rem;
                        padding-right: 1rem;
                      }}
                      .px-2{{
                        padding-left: 2rem;
                        padding-right: 2rem;
                      }}
                      .py-1{{
                        padding-top: 1rem;
                        padding-bottom: 1rem;
                      }}
                      .py-2{{
                        padding-top: 2rem;
                        padding-bottom: 2rem;
                      }}
                      .p-1{{
                        padding: 1rem;
                      }}
                      .p-2{{
                        padding: 2rem;
                      }}
                      .pb-4{{
                        padding-bottom: 4rem;
                      }}
                      
                      .bg-gray{{
                        background-color: #e5e5e5;
                      }}
                      .lowerThen td{{
                        background-color: #f4ad9c;
                      }}
                </style>
            </head>
            <body>
                <!--<div id='content'>-->
                <div id='scrollable'>
                    <table id=""table-gray-border-collapse"" class=""w-full"">
                        <thead>
                            <th class=""align-left p-1 bg-gray"">Name</th>
                            <th class=""align-center p-1 bg-gray"">Total Time in Month</th>
                            <th class=""align-center p-1 bg-gray"">Actions</th>
                        </thead>
                        {tbody}
                    </table>
                </div>
                <div style=""height:50px""></div>
                <img src='/image' alt='pie chart image'/>
                <!--</div>-->
            </body>
        </html>
        ";
        return html;
    }

    static string PageFailure(string message)
    {
        var html=$@"
            <!DOCTYPE html>
            <html>
                <head>
                    <style>
                        *{{
                            padding: 0px;
                            margin: 0px;
                            box-sizing: border-box;
                        }}
                        html{{
                            height: 100vh;
                        }}
                        body{{
                            height: 100%;
                            display:flex;
                            flex-direction: column;
                            align-items: center;
                            justify-content: center;
                        }}
                    </style>
                </head>
                <body>
                    <span>Error occurred, try again later</span>  
                    <span>{message}</span>
                </body>
            </html>
        ";
        return html;
    }

    if (response is TimeEntrySuccess)
    {
        var entries=((TimeEntrySuccess)response).entries;
        return TypedResults.Content(PageSuccess(TimeEntryUtils.CompactTimeEntries(entries)), "text/html");
    }

    if (response is TimeEntryFailure)
    {
        var message=((TimeEntryFailure)response).ErrorMessage;
        return TypedResults.Content(PageFailure(message), "text/html");
    }
    
    return TypedResults.NotFound();
}

async static Task<IResult> TimeEntriesPieChart(HttpClient httpClient)
{
    var code = "vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
    var response = await AzureWebsiteService.GetTimeEntries(httpClient, code);

    static async Task<IResult> ImageSuccess(TimeEntryPerUser[] entries)
    {
        
        using var img = new Image<Rgba32>(600, 600);
        img.Mutate(ctx =>
        {
            ctx.Fill(Color.White);
            int centerX = 300;
            int centerY = 350;
            int radius = 250;
            var colors = new[]
            {
                Color.ParseHex("#36a2eb"),
                Color.ParseHex("#ff6384"),
                Color.ParseHex("#ff9f40"),
                Color.ParseHex("#ffcd56"),
                Color.ParseHex("#4bc0c0"),
                Color.ParseHex("#9966ff"),
                Color.ParseHex("#c9cbcf"),
            };
        
            var sum = 0f;
            foreach (var entry in entries)sum+=entry.TotalWorkingHours;
            float angle = 0f;
            for (int i=0;i<entries.Length;i++)
            {
                var entry=entries[i];
                var totalAngle=(float)entry.TotalWorkingHours/sum*(float)Math.PI*2;
                var points = new PointF[20];
                points[0]=new PointF(centerX,centerY);
                for (int j = 1; j < points.Length; j++)
                {
                    var currentAngle = totalAngle / (points.Length - 2) * (j -1);
                    points[j]=new PointF(
                        (float)(Math.Cos(angle+currentAngle-Math.PI/2)*radius+centerX),
                        (float)(Math.Sin(angle+currentAngle-Math.PI/2)*radius+centerY)
                    );
                }
                ctx.FillPolygon(colors[i%colors.Length],points);
                ctx.DrawPolygon(Color.White,2,points);
                angle+=totalAngle;
            }

            // for (int i = 0; i < entries.Length; i++)
            // {
                // ctx.DrawText()
            // }
        });
    
        var memoryStream = new MemoryStream();
        await img.SaveAsync(memoryStream, new PngEncoder());
        memoryStream.Position = 0;
    
        return TypedResults.File(
            fileContents:memoryStream.ToArray(),
            contentType:"image/png"
        );
    }

    static async Task<IResult> ImageFailure(string message)
    {
        return TypedResults.NotFound(message);
    }
    
    if (response is TimeEntrySuccess)
    {
        var entries=((TimeEntrySuccess)response).entries;
        return await ImageSuccess(TimeEntryUtils.CompactTimeEntries(entries));
    }

    if (response is TimeEntryFailure)
    {
        var message=((TimeEntryFailure)response).ErrorMessage;
        return await ImageFailure(message);
    }
    return TypedResults.NotFound();
}

app.Run();

public class TimeEntryDto
{
    public string Id { get; set; }    
    public string? EmployeeName { get; set; }
    public string StarTimeUtc { get; set; }
    public string EndTimeUtc { get; set; }
    public string EntryNotes { get; set; }
    public string? DeletedOn { get; set; }
}

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

public class AzureWebsiteService
{
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

        static TimeEntry[] compactTimeEntries(TimeEntry[] entries)
        {
            var dict=new Dictionary<string,TimeEntry>();
            foreach (var entry in entries)
            {
                if(entry.EmployeeName==null)continue;
                if (dict.ContainsKey(entry.EmployeeName))
                {
                    var previousEntry=dict[entry.EmployeeName];
                    previousEntry.TotalWorkingHours+=entry.TotalWorkingHours;
                    dict[entry.EmployeeName] = previousEntry;
                }
                else
                {
                    dict[entry.EmployeeName] = entry; 
                }
            }

            var compactedEntries = dict.Values.ToArray();
            var sortedEntries = compactedEntries.OrderByDescending(x=>x.TotalWorkingHours).ToArray();
            return sortedEntries;
        }
        
        try
        {
            var response=await getResponse(http,code);
            var entries=await responseToTimeEntry(response);
            // entries =compactTimeEntries(entries);
            return new TimeEntrySuccess(entries);
        }
        catch (Exception e)
        {
            return new TimeEntryFailure(e.Message);
        }
    }
}

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