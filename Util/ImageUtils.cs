using System.Net.Mime;
using aspnet_task.Model;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace aspnet_task.Utils;

public class ImageUtils
{
    public static Color[] colors = new[]
    {
        Color.ParseHex("#36a2eb"),
        Color.ParseHex("#ff6384"),
        Color.ParseHex("#ff9f40"),
        Color.ParseHex("#ffcd56"),
        Color.ParseHex("#4bc0c0"),
        Color.ParseHex("#9966ff"),
        Color.ParseHex("#c9cbcf"),
    };
    
    public static void DrawPieChart(IImageProcessingContext ctx,TimeEntryPerUser[] entries,int centerX,int centerY,int radius,Color borderColor)
    {
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
            ctx.DrawPolygon(borderColor,2,points);
            angle+=totalAngle;
        }
    }

    public static void DrawLegend(IImageProcessingContext ctx, TimeEntryPerUser[] entries,int columns)
    {
        FontCollection collection = new FontCollection();
        FontFamily family=collection.Add("./Fonts/inter18.ttf");
        Font font=family.CreateFont(12);

        int wordWidth = 150;
        int wordHeight = 20;
        int rectWidth = 20;
        int rectHeight = 10;
        for (int i = 0; i < entries.Length; i++)
        {
            int x=i%columns;
            int y=i/columns;
            var entry = entries[i];
            ctx.FillPolygon(colors[i%colors.Length],new []
            {
                new PointF(x*wordWidth,y*wordHeight),
                new PointF(x*wordWidth+rectWidth,y*wordHeight),
                new PointF(x*wordWidth+rectWidth,y*wordHeight+rectHeight),
                new PointF(x*wordWidth,y*wordHeight+rectHeight),
            });
            ctx.DrawText(entry.EmployeeName,font,Color.Black,new PointF(x*wordWidth+rectWidth+10,y*wordHeight));
        }
    }
}