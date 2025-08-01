using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenData.Mcp.Server.Tools;
using System.ComponentModel;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class InterestsTools(IHttpClientFactory httpClientFactory, ILogger<InterestsTools> logger) : BaseTools(httpClientFactory, logger)
    {
        protected const string InterestsApiBase = "https://interests-api.parliament.uk/api/v1";

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search member's Register of Interests for financial and business declarations | register of interests, ROI, financial interests, conflicts of interest, directorships, consultancies, gifts, external roles, transparency | Use for investigating potential conflicts, researching member finances, or checking declared interests | Returns declared interests including directorships, consultancies, gifts, and other financial interests")]
        public async Task<string> SearchRoiAsync([Description("Parliament member ID | Required: get from member search first | Returns all declared interests")] int memberId)
        {
            var url = $"{InterestsApiBase}/Interests/?MemberId={memberId}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get categories of interests that MPs and Lords must declare in the Register of Interests. Use when you need to understand what types of financial or other interests parliamentarians must declare.")]
        public async Task<string> InterestsCategoriesAsync()
        {
            var url = $"{InterestsApiBase}/Categories";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get list of published Registers of Interests. Use when you need to see all available interest registers or understand the transparency framework for parliamentary interests.")]
        public async Task<string> GetRegistersOfInterestsAsync()
        {
            var url = $"{InterestsApiBase}/Registers";
            return await GetResult(url);
        }
    }
}
