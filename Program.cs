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
    var entries = await AzureWebsiteService.GetTimeEntries(httpClient, code);
    var tbody = new StringBuilder();
    tbody.AppendLine("<tbody>");
    foreach (var entry in entries)
    {
        var className = entry.TotalWorkingHours > 100 ? " class='selected'" : "";
        tbody.AppendLine($@"
            <tr{className}>
                <td class=""align-left p-1"">{entry.EmployeeName}</td>
                <td class=""align-center p-1"">{entry.TotalWorkingHours} hrs</td>
                <td class=""align-center p-1"">Edit</td>
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
                      .selected td{{
                        background-color: #f4ad9c;
                      }}
                </style>
            </head>
            <body>
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

            </body>
        </html>
    ";
    return TypedResults.Content(html, "text/html");
}

async static Task<IResult> TimeEntriesPieChart(HttpClient httpClient)
{
    var code = "vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
    using var img = new Image<Rgba32>(600, 600);
    var entries=await AzureWebsiteService.GetTimeEntries(httpClient,code);
    entries = entries.Take(20).ToArray();
    img.Mutate(ctx =>
    {
        ctx.Fill(Color.White);
        int centerX = 300;
        int centerY = 300;
        int radius = 250;
        var colors = new[]
        {
            Color.Red,
            Color.Green,
            Color.Blue,
            Color.Cyan,
            Color.Orange,
            Color.Purple,
            Color.Violet,
            Color.Tomato,
            Color.Crimson,
            Color.OrangeRed,
        };
        
        var sum = 0;
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
            angle+=totalAngle;
        }
    });
    
    var memoryStream = new MemoryStream();
    await img.SaveAsync(memoryStream, new PngEncoder());
    memoryStream.Position = 0;
    
    return TypedResults.File(
       fileContents:memoryStream.ToArray(),
       contentType:"image/png"
       );
}

static TimeEntry TimeEntryDtoToModel(TimeEntryDto dto)
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

app.Run();

class TimeEntryDto
{
    public string Id { get; set; }    
    public string? EmployeeName { get; set; }
    public string StarTimeUtc { get; set; }
    public string EndTimeUtc { get; set; }
    public string EntryNotes { get; set; }
    public string? DeletedOn { get; set; }
}

class TimeEntry
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
    public int TotalWorkingHours { get; set; }

    private int CalculateTotalWorkingHours()
    {
        return Convert.ToInt32(EndTimeUtc.Subtract(StarTimeUtc).TotalHours);
    }
}


class AzureWebsiteService
{
    private static string apiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api";
    public static async Task<TimeEntry[]> GetTimeEntries(HttpClient http,string code)
    {
        var response = await http.GetAsync($"{apiUrl}/gettimeentries?code={code}");
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var json=await response.Content.ReadAsStringAsync();
            var entryDtos = JsonConvert.DeserializeObject<TimeEntryDto[]>(json) ?? new TimeEntryDto[0];
            var entries=new  TimeEntry[entryDtos.Length];
            for (int i = 0; i < entryDtos.Length; i++) entries[i]=TimeEntryDtoToModel(entryDtos[i]);
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
            return dict.Values.ToArray();
        }
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Console.WriteLine("No time entries found");
        }
        return new TimeEntry[0];
    }
    
    private static TimeEntry TimeEntryDtoToModel(TimeEntryDto dto)
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
}