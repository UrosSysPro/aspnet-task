namespace aspnet_task.View;

public class IndexFailure
{
    public static string view(string message)
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
}