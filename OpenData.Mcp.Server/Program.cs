using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.
    AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddMemoryCache();

var app = builder.Build();

await app.RunAsync();

[McpServerToolType]
public static class UKParliamentOpenDataTool
{
    [McpServerTool, Description("get information on member of parliament by name")]
    public static async Task<string> GetMemberByNameAsync(string name)
    {
        return $"You asked for information on the member of parliament {name}. This method is under construction";
    }
}