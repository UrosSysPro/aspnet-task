using aspnet_task.Controller;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseHttpsRedirection();
app.MapGet("/", WebsiteController.Index);
app.MapGet("/image", PieChartController.Image);

app.Run();