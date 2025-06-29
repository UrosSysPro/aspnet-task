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
}