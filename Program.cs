using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Drawing.Processing;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseHttpsRedirection();
app.UseRouting();
app.MapRazorPages();

app.MapGet("/", TimeEntriesPage);
app.MapGet("/image", TimeEntriesPieChart);

async static Task<IResult> TimeEntriesPage()
{
    return TypedResults.Ok("hello world");
}

async static Task<IResult> TimeEntriesPieChart()
{
    using var img = new Image<Rgba32>(600, 600);
    img.Mutate(ctx => ctx.Fill(Color.Red));
    
    var memoryStream = new MemoryStream();
    await img.SaveAsync(memoryStream, new PngEncoder());
    memoryStream.Position = 0;
    
    return TypedResults.File(
       fileContents:memoryStream.ToArray(),
       contentType:"image/png"
       );
}

app.Run();
