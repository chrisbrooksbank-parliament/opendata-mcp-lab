using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenData.Mcp.Server.Tools;
using System.ComponentModel;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class StatutoryInstrumentsTools(IHttpClientFactory httpClientFactory, ILogger<StatutoryInstrumentsTools> logger) : BaseTools(httpClientFactory, logger)
    {
        protected const string StatutoryInstrumentsApiBase = "https://statutoryinstruments-api.parliament.uk/api/v2";

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for Statutory Instruments (secondary legislation) by name. Use when researching government regulations, rules, or orders made under primary legislation. SIs are used to implement or modify laws.")]
        public async Task<string> SearchStatutoryInstrumentsAsync([Description("Name or title of the statutory instrument to search for")] string name)
        {
            var url = $"{StatutoryInstrumentsApiBase}/StatutoryInstrument?Name={Uri.EscapeDataString(name)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for Acts of Parliament (primary legislation) by name or topic. Use when researching existing laws, finding legislation on specific subjects, or understanding the legal framework on particular issues.")]
        public async Task<string> SearchActsOfParliamentAsync([Description("Name or title of the Act to search for (e.g. 'Climate Change Act', 'Human Rights Act')")] string name)
        {
            var url = $"{StatutoryInstrumentsApiBase}/ActOfParliament?Name={Uri.EscapeDataString(name)}";
            return await GetResult(url);
        }
    }
}
