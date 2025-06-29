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
        tbody.AppendLine($@"
            <tr>
                <td>{entry.EmployeeName}</td>
                <td>{entry.TotalWorkingHours()} hrs</td>
                <td>Edit</td>
            </tr>
        ");
    }
    tbody.AppendLine("</tbody>");
    var html = $@"
        <!DOCTYPE html> 
        <html>
            <head>
            </head>
            <body>
                <table>
                    <thead>
                        <th>Name</th>
                        <th>Total time in month</th>
                        <th>Edit</th>
                    </thead>
                    {tbody}
                </table>
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
        foreach (var entry in entries)sum+=entry.TotalWorkingHours();
        float angle = 0f;
        for (int i=0;i<entries.Length;i++)
        {
            var entry=entries[i];
            var totalAngle=(float)entry.TotalWorkingHours()/sum*(float)Math.PI*2;
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
    public string EmployeeName { get; set; }
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
    }
    public string Id { get; set; }
    public string EmployeeName { get; set; }
    public DateTime StarTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public string EntryNotes { get; set; }
    public DateTime? DeletedOn { get; set; }

    public int TotalWorkingHours()
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
            return entries;
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