using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenData.Mcp.Server.Tools;
using System.ComponentModel;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class LordsVotesTools(IHttpClientFactory httpClientFactory, ILogger<LordsVotesTools> logger) : BaseTools(httpClientFactory, logger)
    {
        protected const string LordsVotesApiBase = "http://lordsvotes-api.parliament.uk/data";

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search House of Lords voting records (divisions). Use when you want to find how Lords voted on specific issues, bills, or amendments in the upper chamber.")]
        public async Task<string> SearchLordsDivisionsAsync([Description("Search term for Lords division topics (e.g. 'brexit', 'climate', 'NHS')")] string searchTerm)
        {
            var url = $"{LordsVotesApiBase}/divisions/search?queryParameters.searchTerm={Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get complete voting record of a Lord in House of Lords divisions. Use when analyzing how a specific Lord votes, their voting patterns, or their stance on particular issues through their voting history.")]
        public async Task<string> GetLordsVotingRecordForMemberAsync([Description("Parliament member ID to get Lords voting record for")] int memberId, [Description("Optional: search term to filter divisions")] string searchTerm = null, [Description("Optional: include votes where member was a teller")] bool? includeWhenMemberWasTeller = null, [Description("Optional: start date in YYYY-MM-DD format")] string startDate = null, [Description("Optional: end date in YYYY-MM-DD format")] string endDate = null, [Description("Optional: specific division number")] int? divisionNumber = null, [Description("Number of records to skip (for pagination)")] int skip = 0, [Description("Number of records to return (default 25, max 100)")] int take = 25)
        {
            var url = BuildUrl($"{LordsVotesApiBase}/Divisions/membervoting", new()
            {
                ["MemberId"] = memberId.ToString(),
                ["SearchTerm"] = searchTerm,
                ["IncludeWhenMemberWasTeller"] = includeWhenMemberWasTeller?.ToString(),
                ["StartDate"] = startDate,
                ["EndDate"] = endDate,
                ["DivisionNumber"] = divisionNumber?.ToString(),
                ["skip"] = skip.ToString(),
                ["take"] = take.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get detailed information about a specific House of Lords division by ID. Use when you need complete details about a particular Lords vote including who voted content/not content, tellers, and voting totals.")]
        public async Task<string> GetLordsDivisionByIdAsync([Description("Unique Lords division ID number")] int divisionId)
        {
            var url = $"{LordsVotesApiBase}/Divisions/{divisionId}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get House of Lords divisions grouped by party voting patterns. Use when analyzing how different parties voted on issues in the Lords or understanding party-line voting behavior. Shows vote counts by party rather than individual Lords.")]
        public async Task<string> GetLordsDivisionsGroupedByPartyAsync([Description("Optional: search term to filter divisions")] string searchTerm = null, [Description("Optional: member ID to filter divisions")] int? memberId = null, [Description("Optional: start date in YYYY-MM-DD format")] string startDate = null, [Description("Optional: end date in YYYY-MM-DD format")] string endDate = null, [Description("Optional: specific division number")] int? divisionNumber = null, [Description("Optional: include when member was a teller")] bool? includeWhenMemberWasTeller = null)
        {
            var url = BuildUrl($"{LordsVotesApiBase}/Divisions/groupedbyparty", new()
            {
                ["SearchTerm"] = searchTerm,
                ["MemberId"] = memberId?.ToString(),
                ["StartDate"] = startDate,
                ["EndDate"] = endDate,
                ["DivisionNumber"] = divisionNumber?.ToString(),
                ["IncludeWhenMemberWasTeller"] = includeWhenMemberWasTeller?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get total count of House of Lords divisions matching search criteria. Use when you need to know how many Lords divisions match your search parameters before retrieving the actual results.")]
        public async Task<string> GetLordsDivisionsSearchCountAsync([Description("Optional: search term to filter divisions")] string searchTerm = null, [Description("Optional: member ID to filter divisions")] int? memberId = null, [Description("Optional: start date in YYYY-MM-DD format")] string startDate = null, [Description("Optional: end date in YYYY-MM-DD format")] string endDate = null, [Description("Optional: specific division number")] int? divisionNumber = null, [Description("Optional: include when member was a teller")] bool? includeWhenMemberWasTeller = null)
        {
            var url = BuildUrl($"{LordsVotesApiBase}/Divisions/searchTotalResults", new()
            {
                ["SearchTerm"] = searchTerm,
                ["MemberId"] = memberId?.ToString(),
                ["StartDate"] = startDate,
                ["EndDate"] = endDate,
                ["DivisionNumber"] = divisionNumber?.ToString(),
                ["IncludeWhenMemberWasTeller"] = includeWhenMemberWasTeller?.ToString()
            });
            return await GetResult(url);
        }
    }
}
