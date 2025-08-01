using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenData.Mcp.Server.Tools;
using System.ComponentModel;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class WhatsOnTools(IHttpClientFactory httpClientFactory, ILogger<WhatsOnTools> logger) : BaseTools(httpClientFactory, logger)
    {
        protected const string WhatsonApiBase = "https://whatson-api.parliament.uk/calendar";

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search parliamentary calendar for upcoming events and business in either chamber. Use when you want to know what's scheduled in Parliament, upcoming debates, or future parliamentary business. House: Commons/Lords.")]
        public async Task<string> SearchCalendar([Description("House name: 'Commons' or 'Lords'")] string house, [Description("Start date in YYYY-MM-DD format")] string startDate, [Description("End date in YYYY-MM-DD format")] string endDate)
        {
            var url = BuildUrl($"{WhatsonApiBase}/events/list.json", new()
            {
                ["queryParameters.house"] = house,
                ["queryParameters.startDate"] = startDate,
                ["queryParameters.endDate"] = endDate
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get list of parliamentary sessions. Use when you need to understand parliamentary terms, session dates, or the parliamentary calendar structure.")]
        public async Task<string> GetSessions()
        {
            var url = $"{WhatsonApiBase}/sessions/list.json";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get periods when Parliament is not sitting (recesses, holidays). Use when you need to know when Parliament is on break, recess periods, or when no parliamentary business is scheduled.")]
        public async Task<string> GetNonSittingDays([Description("House name: 'Commons' or 'Lords'")] string house, [Description("Start date in YYYY-MM-DD format")] string startDate, [Description("End date in YYYY-MM-DD format")] string endDate)
        {
            var url = BuildUrl($"{WhatsonApiBase}/events/nonsitting.json", new()
            {
                ["queryParameters.house"] = house,
                ["queryParameters.startDate"] = startDate,
                ["queryParameters.endDate"] = endDate
            });
            return await GetResult(url);
        }
    }
}
