using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenData.Mcp.Server.Tools;
using System.ComponentModel;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class NowTools(IHttpClientFactory httpClientFactory, ILogger<NowTools> logger) : BaseTools(httpClientFactory, logger)
    {
        protected const string NowApiBase = "https://now-api.parliament.uk/api";

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get live information about what's currently happening in the House of Commons chamber. Use when you want real-time updates on parliamentary business, current debates, or voting activity.")]
        public async Task<string> HappeningNowInCommonsAsync()
        {
            var url = $"{NowApiBase}/Message/message/CommonsMain/current";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get live information about what's currently happening in the House of Lords chamber. Use when you want real-time updates on Lords business, current debates, or voting activity.")]
        public async Task<string> HappeningNowInLordsAsync()
        {
            var url = $"{NowApiBase}/Message/message/LordsMain/current";
            return await GetResult(url);
        }
    }
}
