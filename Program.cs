var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseHttpsRedirection();
app.UseRouting();
app.MapGet("/", Hello);
app.MapRazorPages();

async static Task<IResult> Hello()
{
    return TypedResults.Ok("hello world");
}

app.Run();
