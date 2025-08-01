using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenData.Mcp.Server.Tools;
using System.ComponentModel;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class CommonsVotesTools(IHttpClientFactory httpClientFactory, ILogger<CommonsVotesTools> logger) : BaseTools(httpClientFactory, logger)
    {
        protected const string CommonsVotesApiBase = "http://commonsvotes-api.parliament.uk/data";

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search House of Commons voting records (divisions). Use when you want to find how MPs voted on specific issues, bills, or amendments. Can filter by member, date range, or division number.")]
        public async Task<string> SearchCommonsDivisionsAsync([Description("Search term for division topics (e.g. 'brexit', 'climate', 'NHS')")] string searchTerm, [Description("Optional: specific member ID to filter votes")] int? memberId = null, [Description("Optional: start date in YYYY-MM-DD format")] string startDate = null, [Description("Optional: end date in YYYY-MM-DD format")] string endDate = null, [Description("Optional: specific division number")] int? divisionNumber = null)
        {
            var url = BuildUrl($"{CommonsVotesApiBase}/divisions.json/search", new()
            {
                ["queryParameters.searchTerm"] = searchTerm,
                ["memberId"] = memberId?.ToString(),
                ["queryParameters.startDate"] = startDate,
                ["queryParameters.endDate"] = endDate,
                ["queryParameters.divisionNumber"] = divisionNumber?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get complete voting record of an MP in House of Commons divisions. Use when analyzing how a specific MP votes, their voting patterns, or their stance on particular issues through their voting history.")]
        public async Task<string> GetCommonsVotingRecordForMemberAsync([Description("Parliament member ID to get Commons voting record for")] int memberId)
        {
            var url = $"{CommonsVotesApiBase}/divisions.json/membervoting?queryParameters.memberId={memberId}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get detailed information about a specific House of Commons division by ID. Use when you need complete details about a particular vote including who voted for/against, tellers, and voting totals.")]
        public async Task<string> GetCommonsDivisionByIdAsync([Description("Unique Commons division ID number")] int divisionId)
        {
            var url = $"{CommonsVotesApiBase}/division/{divisionId}.json";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get House of Commons divisions grouped by party voting patterns. Use when analyzing how different parties voted on issues or understanding party-line voting behavior. Shows vote counts by party rather than individual MPs.")]
        public async Task<string> GetCommonsDivisionsGroupedByPartyAsync([Description("Optional: search term to filter divisions")] string searchTerm = null, [Description("Optional: member ID to filter divisions")] int? memberId = null, [Description("Optional: start date in YYYY-MM-DD format")] string startDate = null, [Description("Optional: end date in YYYY-MM-DD format")] string endDate = null, [Description("Optional: specific division number")] int? divisionNumber = null, [Description("Optional: include when member was a teller")] bool? includeWhenMemberWasTeller = null)
        {
            var url = BuildUrl($"{CommonsVotesApiBase}/divisions.json/groupedbyparty", new()
            {
                ["queryParameters.searchTerm"] = searchTerm,
                ["queryParameters.memberId"] = memberId?.ToString(),
                ["queryParameters.startDate"] = startDate,
                ["queryParameters.endDate"] = endDate,
                ["queryParameters.divisionNumber"] = divisionNumber?.ToString(),
                ["queryParameters.includeWhenMemberWasTeller"] = includeWhenMemberWasTeller?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get total count of House of Commons divisions matching search criteria. Use when you need to know how many divisions match your search parameters before retrieving the actual results.")]
        public async Task<string> GetCommonsDivisionsSearchCountAsync([Description("Optional: search term to filter divisions")] string searchTerm = null, [Description("Optional: member ID to filter divisions")] int? memberId = null, [Description("Optional: start date in YYYY-MM-DD format")] string startDate = null, [Description("Optional: end date in YYYY-MM-DD format")] string endDate = null, [Description("Optional: specific division number")] int? divisionNumber = null, [Description("Optional: include when member was a teller")] bool? includeWhenMemberWasTeller = null)
        {
            var url = BuildUrl($"{CommonsVotesApiBase}/divisions.json/searchTotalResults", new()
            {
                ["queryParameters.searchTerm"] = searchTerm,
                ["queryParameters.memberId"] = memberId?.ToString(),
                ["queryParameters.startDate"] = startDate,
                ["queryParameters.endDate"] = endDate,
                ["queryParameters.divisionNumber"] = divisionNumber?.ToString(),
                ["queryParameters.includeWhenMemberWasTeller"] = includeWhenMemberWasTeller?.ToString()
            });
            return await GetResult(url);
        }
    }
}
