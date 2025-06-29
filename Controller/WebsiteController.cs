using aspnet_task.View;

namespace aspnet_task.Controller;

using System.Text;
using aspnet_task.Model;
using aspnet_task.Service;
using aspnet_task.Utils;
using static aspnet_task.Service.AzureWebsiteService;

public class WebsiteController
{
    public static async Task<IResult> Index(HttpClient httpClient)
    {
        var code = "vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
        var response = await AzureWebsiteService.GetTimeEntries(httpClient, code);

        if (response is TimeEntrySuccess)
        {
            var entries=((TimeEntrySuccess)response).entries;
            return TypedResults.Content(IndexSuccess.view(TimeEntryUtils.CompactTimeEntries(entries)), "text/html");
        }

        if (response is TimeEntryFailure)
        {
            var message=((TimeEntryFailure)response).ErrorMessage;
            return TypedResults.Content(IndexFailure.view(message), "text/html");
        }
    
        return TypedResults.NotFound();
    }
}