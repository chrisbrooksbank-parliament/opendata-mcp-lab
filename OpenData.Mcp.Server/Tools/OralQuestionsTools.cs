using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenData.Mcp.Server.Tools;
using System.ComponentModel;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class OralQuestionsTools(IHttpClientFactory httpClientFactory, ILogger<OralQuestionsTools> logger) : BaseTools(httpClientFactory, logger)
    {
        protected const string OralQuestionsApiBase = "https://oralquestionsandmotions-api.parliament.uk";

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get recently tabled Early Day Motions - formal political statements | EDMs, political motions, backbench initiatives, MP opinions, parliamentary statements, political positions, cross-party support | Use for tracking political sentiment, finding MP stances on issues, monitoring backbench activity, or researching political movements | Returns recent EDMs with titles, sponsors, supporters, and tabling dates | Data freshness: updated daily")]
        public async Task<string> GetRecentlyTabledEdmsAsync([Description("Number of EDMs to return | Default: 10, recommended max: 50 | Recent motions returned first")] int take = 10)
        {
            var url = $"{OralQuestionsApiBase}/EarlyDayMotions/list?parameters.orderBy=DateTabledDesc&skip=0&take={take}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search Early Day Motions by topic or keyword. Use when researching MP opinions on specific issues or finding motions related to particular subjects. EDMs often reflect backbench MP concerns.")]
        public async Task<string> SearchEarlyDayMotionsAsync([Description("Search term for EDM topics or content (e.g. 'climate change', 'NHS funding')")] string searchTerm)
        {
            var url = $"{OralQuestionsApiBase}/EarlyDayMotions/list?parameters.searchTerm={Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get scheduled oral question times for ministers in Parliament. Use when you want to know when specific departments will answer questions or when particular topics will be discussed.")]
        public async Task<string> SearchOralQuestionTimesAsync([Description("Start date for question times in YYYY-MM-DD format")] string answeringDateStart, [Description("End date for question times in YYYY-MM-DD format")] string answeringDateEnd)
        {
            var url = $"{OralQuestionsApiBase}/oralquestiontimes/list?parameters.answeringDateStart={Uri.EscapeDataString(answeringDateStart)}&parameters.answeringDateEnd={Uri.EscapeDataString(answeringDateEnd)}";
            return await GetResult(url);
        }
    }
}
