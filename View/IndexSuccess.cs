﻿namespace aspnet_task.View;

using System.Text;
using aspnet_task.Model;

public class IndexSuccess
{
    
    public static string view(TimeEntryPerUser[] entries)
    {
        var tbody = new StringBuilder();
        tbody.AppendLine("<tbody>");
        foreach (var entry in entries)
        {
            var className = entry.TotalWorkingHours < 100 ? " class='lowerThen'" : "";
            var name = entry.EmployeeName;
            var workingHours = (int)Math.Round(entry.TotalWorkingHours);
            tbody.AppendLine($@"
            <tr{className}>
                <td class=""align-left p-1"">{name}</td>
                <td class=""align-center p-1"">{workingHours} hrs</td>
                <td class=""align-center p-1""><a href='#'>Edit</a></td>
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
                    #content{{
                        padding-top:10rem;
                        padding-bottom:10rem;
                        display:flex;
                        flex-direction: column;
                        align-items: center;
                        justify-content: center;
                    }}
                    body{{
                        overflow-y:auto;
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
                      .lowerThen td{{
                        background-color: #f4ad9c;
                      }}
                </style>
            </head>
            <body>
                <div id='content'>
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
                </div>
            </body>
        </html>
        ";
        return html;
    }
}