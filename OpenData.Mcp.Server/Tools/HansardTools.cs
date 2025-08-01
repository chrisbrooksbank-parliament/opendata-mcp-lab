using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenData.Mcp.Server.Tools;
using System.ComponentModel;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class HansardTools(IHttpClientFactory httpClientFactory, ILogger<HansardTools> logger) : BaseTools(httpClientFactory, logger)
    {
        protected const string HansardApiBase = "https://hansard-api.parliament.uk";

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search Hansard (official parliamentary record) for speeches and debates. Use when researching what was said in Parliament on specific topics, by specific members, or in specific time periods. House: 1=Commons, 2=Lords.")]
        public async Task<string> SearchHansardAsync([Description("House number: 1 for Commons, 2 for Lords")] int house, [Description("Start date in YYYY-MM-DD format")] string startDate, [Description("End date in YYYY-MM-DD format")] string endDate, [Description("Search term for speeches or debates (e.g. 'climate change', 'NHS')")] string searchTerm)
        {
            var url = BuildUrl($"{HansardApiBase}/search.json", new()
            {
                ["queryParameters.house"] = house.ToString(),
                ["queryParameters.startDate"] = startDate,
                ["queryParameters.endDate"] = endDate,
                ["queryParameters.searchTerm"] = searchTerm
            });
            return await GetResult(url);
        }
    }
}
