using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenData.Mcp.Server.Tools;
using System.ComponentModel;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class TreatiesTools(IHttpClientFactory httpClientFactory, ILogger<TreatiesTools> logger) : BaseTools(httpClientFactory, logger)
    {
        protected const string TreatiesApiBase = "https://treaties-api.parliament.uk/api";

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search UK international treaties and agreements under parliamentary scrutiny | treaties, international agreements, trade deals, diplomatic treaties, international law, bilateral agreements | Use for researching international relations, trade agreements, or diplomatic commitments | Returns treaty details including titles, countries involved, and parliamentary scrutiny status")]
        public async Task<string> SearchTreatiesAsync([Description("Search term for treaties | Examples: 'trade', 'EU', 'climate', 'Brexit' | Searches titles and content")] string searchText)
        {
            var url = $"{TreatiesApiBase}/Treaty?SearchText={Uri.EscapeDataString(searchText)}";
            return await GetResult(url);
        }
    }
}
