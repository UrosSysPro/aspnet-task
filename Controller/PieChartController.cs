namespace aspnet_task.Controller;

using aspnet_task.Model;
using aspnet_task.Utils;
using aspnet_task.Service;
using static aspnet_task.Service.AzureWebsiteService;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Drawing.Processing;

public class PieChartController
{
    public static async Task<IResult> Image(HttpClient httpClient)
    {
        var code = "vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
        var response = await AzureWebsiteService.GetTimeEntries(httpClient, code);

        static async Task<IResult> ImageSuccess(TimeEntryPerUser[] entries)
        {
            using var img = new Image<Rgba32>(600, 600);
            img.Mutate(ctx =>
            {
                ctx.Fill(Color.White);
                ImageUtils.DrawPieChart(ctx,entries,300,350,250,Color.White);
                ImageUtils.DrawLegend(ctx,entries,4);
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
}