using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace OpenData.Mcp.Server.Tools
{
    [McpServerToolType]
    public class BillsTools(IHttpClientFactory httpClientFactory, ILogger<BillsTools> logger) : BaseTools(httpClientFactory, logger)
    {
        protected const string BillsApiBase = "https://bills-api.parliament.uk/api/v1";

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get most recently updated bills and current legislative activity | recent bills, new legislation, latest laws, parliamentary bills, legislative updates, current proposals | Use for tracking new legislation, monitoring bill progress, or finding recently introduced/updated laws | Returns bill titles, stages, sponsors, dates, and current status | Data freshness: updated frequently")]
        public async Task<string> GetRecentlyUpdatedBillsAsync([Description("Number of bills to return | Default: 10, recommended max: 50 | Higher numbers may slow response")] int take = 10)
        {
            var url = $"{BillsApiBase}/Bills?SortOrder=DateUpdatedDescending&skip=0&take={take}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for parliamentary bills by title, subject, or keyword. Use when researching proposed legislation, finding bills on specific topics, or tracking legislative progress.")]
        public async Task<string> SearchBillsAsync([Description("Search term for bill titles or content (e.g. 'environment', 'health', 'finance')")] string searchTerm, [Description("Optional: member ID to filter bills sponsored by specific member")] int? memberId = null)
        {
            var url = $"{BillsApiBase}/Bills?SearchTerm={Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get all types of bills that can be introduced in Parliament (e.g., Government Bill, Private Member's Bill). Use when you need to understand different categories of legislation.")]
        public async Task<string> BillTypesAsync()
        {
            var url = $"{BillsApiBase}/BillTypes";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get all possible stages a bill can go through in its legislative journey. Use when tracking bill progress or understanding the legislative process (e.g., First Reading, Committee Stage, Royal Assent).")]
        public async Task<string> BillStagesAsync()
        {
            var url = $"{BillsApiBase}/Stages";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get detailed information about a specific bill by ID. Use when you need comprehensive bill details including title, sponsors, stages, summary, and current status.")]
        public async Task<string> GetBillByIdAsync([Description("Unique bill ID number")] int billId)
        {
            var url = $"{BillsApiBase}/Bills/{billId}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get all stages of a specific bill by bill ID. Use when tracking a bill's progress through Parliament, understanding its legislative journey, or finding specific stages like Committee Stage or Third Reading.")]
        public async Task<string> GetBillStagesAsync([Description("Bill ID to get stages for")] int billId, [Description("Optional: number of records to skip (for pagination)")] int? skip = null, [Description("Optional: number of records to return")] int? take = null)
        {
            var url = BuildUrl($"{BillsApiBase}/Bills/{billId}/Stages", new()
            {
                ["Skip"] = skip?.ToString(),
                ["Take"] = take?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get detailed information about a specific stage of a bill. Use when you need complete details about a particular stage including timings, committee involvement, and related activities.")]
        public async Task<string> GetBillStageDetailsAsync([Description("Bill ID")] int billId, [Description("Bill stage ID to get details for")] int billStageId)
        {
            var url = $"{BillsApiBase}/Bills/{billId}/Stages/{billStageId}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get all amendments for a specific bill stage. Use when researching proposed changes to legislation, tracking amendment activity, or understanding what modifications are being suggested to a bill.")]
        public async Task<string> GetBillStageAmendmentsAsync([Description("Bill ID")] int billId, [Description("Bill stage ID to get amendments for")] int billStageId, [Description("Optional: search term for amendment content")] string searchTerm = null, [Description("Optional: specific amendment number")] string amendmentNumber = null, [Description("Optional: amendment decision status")] string decision = null, [Description("Optional: member ID who proposed amendment")] int? memberId = null, [Description("Optional: number of records to skip (for pagination)")] int? skip = null, [Description("Optional: number of records to return")] int? take = null)
        {
            var url = BuildUrl($"{BillsApiBase}/Bills/{billId}/Stages/{billStageId}/Amendments", new()
            {
                ["SearchTerm"] = searchTerm,
                ["AmendmentNumber"] = amendmentNumber,
                ["Decision"] = decision,
                ["MemberId"] = memberId?.ToString(),
                ["Skip"] = skip?.ToString(),
                ["Take"] = take?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get detailed information about a specific amendment. Use when you need complete amendment details including text, sponsors, decision, and explanatory notes.")]
        public async Task<string> GetAmendmentByIdAsync([Description("Bill ID")] int billId, [Description("Bill stage ID")] int billStageId, [Description("Amendment ID to get details for")] int amendmentId)
        {
            var url = $"{BillsApiBase}/Bills/{billId}/Stages/{billStageId}/Amendments/{amendmentId}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get ping pong items (amendments and motions) for a bill stage. Use when researching the final stages of bills passing between Commons and Lords, including disagreements and agreements on amendments.")]
        public async Task<string> GetBillStagePingPongItemsAsync([Description("Bill ID")] int billId, [Description("Bill stage ID to get ping pong items for")] int billStageId, [Description("Optional: search term for ping pong item content")] string searchTerm = null, [Description("Optional: specific amendment number")] string amendmentNumber = null, [Description("Optional: ping pong item decision status")] string decision = null, [Description("Optional: member ID who proposed item")] int? memberId = null, [Description("Optional: number of records to skip (for pagination)")] int? skip = null, [Description("Optional: number of records to return")] int? take = null)
        {
            var url = BuildUrl($"{BillsApiBase}/Bills/{billId}/Stages/{billStageId}/PingPongItems", new()
            {
                ["SearchTerm"] = searchTerm,
                ["AmendmentNumber"] = amendmentNumber,
                ["Decision"] = decision,
                ["MemberId"] = memberId?.ToString(),
                ["Skip"] = skip?.ToString(),
                ["Take"] = take?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get detailed information about a specific ping pong item (amendment or motion). Use when you need complete details about final stage amendments or motions in the legislative process.")]
        public async Task<string> GetPingPongItemByIdAsync([Description("Bill ID")] int billId, [Description("Bill stage ID")] int billStageId, [Description("Ping pong item ID to get details for")] int pingPongItemId)
        {
            var url = $"{BillsApiBase}/Bills/{billId}/Stages/{billStageId}/PingPongItems/{pingPongItemId}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get all publications for a specific bill. Use when researching bill documents, impact assessments, explanatory notes, or tracking document versions throughout the legislative process.")]
        public async Task<string> GetBillPublicationsAsync([Description("Bill ID to get publications for")] int billId)
        {
            var url = $"{BillsApiBase}/Bills/{billId}/Publications";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get publications for a specific bill stage. Use when you need documents related to a particular stage of legislation, such as committee reports or stage-specific amendments.")]
        public async Task<string> GetBillStagePublicationsAsync([Description("Bill ID")] int billId, [Description("Stage ID to get publications for")] int stageId)
        {
            var url = $"{BillsApiBase}/Bills/{billId}/Stages/{stageId}/Publications";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get information about a specific publication document. Use when you need metadata about bill documents including filename, content type, and size.")]
        public async Task<string> GetPublicationDocumentAsync([Description("Publication ID")] int publicationId, [Description("Document ID to get details for")] int documentId)
        {
            var url = $"{BillsApiBase}/Publications/{publicationId}/Documents/{documentId}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get news articles related to a specific bill. Use when researching media coverage, press releases, or official communications about legislation.")]
        public async Task<string> GetBillNewsArticlesAsync([Description("Bill ID to get news articles for")] int billId, [Description("Optional: number of records to skip (for pagination)")] int? skip = null, [Description("Optional: number of records to return")] int? take = null)
        {
            var url = BuildUrl($"{BillsApiBase}/Bills/{billId}/NewsArticles", new()
            {
                ["Skip"] = skip?.ToString(),
                ["Take"] = take?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get RSS feed of all bills. Use when you want to stay updated on all legislative activity through RSS feeds.")]
        public async Task<string> GetAllBillsRssAsync()
        {
            var url = $"{BillsApiBase}/Rss/allbills.rss";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get RSS feed of public bills only. Use when you want to monitor government and public bills through RSS feeds, excluding private bills.")]
        public async Task<string> GetPublicBillsRssAsync()
        {
            var url = $"{BillsApiBase}/Rss/publicbills.rss";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get RSS feed of private bills only. Use when you want to monitor private member bills and private bills through RSS feeds.")]
        public async Task<string> GetPrivateBillsRssAsync()
        {
            var url = $"{BillsApiBase}/Rss/privatebills.rss";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get RSS feed for a specific bill by ID. Use when you want to track updates and changes to a particular piece of legislation through RSS feeds.")]
        public async Task<string> GetBillRssAsync([Description("Bill ID to get RSS feed for")] int billId)
        {
            var url = $"{BillsApiBase}/Rss/Bills/{billId}.rss";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get all publication types available for bills. Use when you need to understand the different types of documents that can be associated with legislation.")]
        public async Task<string> GetPublicationTypesAsync([Description("Optional: number of records to skip (for pagination)")] int? skip = null, [Description("Optional: number of records to return")] int? take = null)
        {
            var url = BuildUrl($"{BillsApiBase}/PublicationTypes", new()
            {
                ["Skip"] = skip?.ToString(),
                ["Take"] = take?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get parliamentary sittings with optional filtering by house and date range. Use when researching when Parliament was in session, finding specific sitting dates, or tracking parliamentary activity.")]
        public async Task<string> GetSittingsAsync([Description("Optional: house name ('Commons' or 'Lords')")] string house = null, [Description("Optional: start date in YYYY-MM-DD format")] string dateFrom = null, [Description("Optional: end date in YYYY-MM-DD format")] string dateTo = null, [Description("Optional: number of records to skip (for pagination)")] int? skip = null, [Description("Optional: number of records to return")] int? take = null)
        {
            var url = BuildUrl($"{BillsApiBase}/Sittings", new()
            {
                ["House"] = house,
                ["DateFrom"] = dateFrom,
                ["DateTo"] = dateTo,
                ["Skip"] = skip?.ToString(),
                ["Take"] = take?.ToString()
            });
            return await GetResult(url);
        }
    }
}
