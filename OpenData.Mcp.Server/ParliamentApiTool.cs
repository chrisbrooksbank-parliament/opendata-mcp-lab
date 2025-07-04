using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.Logging;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class ParliamentApiTool(IHttpClientFactory httpClientFactory, ILogger<ParliamentApiTool> logger)
    {
        private const string MembersApiBase = "https://members-api.parliament.uk/api";
        private const string BillsApiBase = "https://bills-api.parliament.uk/api/v1";
        private const string OralQuestionsApiBase = "https://oralquestionsandmotions-api.parliament.uk";
        private const string CommitteesApiBase = "https://committees-api.parliament.uk/api";
        private const string InterestsApiBase = "https://interests-api.parliament.uk/api/v1";
        private const string ErskineMayApiBase = "https://erskinemay-api.parliament.uk/api";
        private const string CommonsVotesApiBase = "http://commonsvotes-api.parliament.uk/data";
        private const string LordsVotesApiBase = "http://lordsvotes-api.parliament.uk/data";
        private const string NowApiBase = "https://now-api.parliament.uk/api";
        private const string StatutoryInstrumentsApiBase = "https://statutoryinstruments-api.parliament.uk/api/v2";
        private const string HansardApiBase = "https://hansard-api.parliament.uk";
        private const string WhatsonApiBase = "https://whatson-api.parliament.uk/calendar";
        private const string TreatiesApiBase = "https://treaties-api.parliament.uk/api";
        
        private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(30);
        private const int MaxRetryAttempts = 3;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(1);

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Say Hello to parliament. Get the recommended system prompt for optimal use of this UK Parliament MCP server. Use this first to understand how to properly interact with parliamentary data APIs.")]
        public async Task<string> HelloParliament()
        {
            const string systemPrompt = @"You are a helpful assistant that answers questions using only data from UK Parliament MCP servers.
                When the session begins, introduce yourself with a brief message such as:
                ""Hello! I'm a parliamentary data assistant. I can help answer questions using official data from the UK Parliament MCP APIs. Just ask me something, and I'll fetch what I can — and I'll always show you which sources I used.""
                When responding to user queries, you must:
                Only retrieve and use data from the MCP API endpoints this server provides.
                Avoid using any external sources or inferred knowledge.
                After every response, append a list of all MCP API URLs used to generate the answer.
                If no relevant data is available via the MCP API, state that clearly and do not attempt to fabricate a response.
                Convert raw data into human-readable summaries while preserving accuracy, but always list the raw URLs used.";
            return systemPrompt;
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Say Goodbye to parliament. Undo the system prompt so the user interacts as normal, without special restrictions or guidance.")]
        public async Task<string> GoodByeParliament()
        {
            const string systemPrompt = @"You are now interacting as a normal assistant. There are no special restrictions or requirements for using UK Parliament MCP data. You may answer questions using any available data or knowledge, and you do not need to append MCP API URLs or limit yourself to MCP sources. Resume normal assistant behavior.";
            return systemPrompt;
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Find UK Parliament member details by searching their name. Use when you need to look up MPs, Lords, or former members by name. Returns biographical information, current roles, and party affiliation.")]
        public async Task<string> GetMemberByNameAsync(IMcpServer thisServer, string name)
        {
            var url = $"{MembersApiBase}/Members/Search?Name={Uri.EscapeDataString(name)}";
            var content = await GetResult(url);
            return content;
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get the most recently updated bills in Parliament. Use when you want current legislative activity or recent changes to proposed laws. Returns bill titles, stages, sponsors, and last update dates.")]
        public async Task<string> GetRecentlyUpdatedBillsAsync(int take = 10)
        {
            var url = $"{BillsApiBase}/Bills?SortOrder=DateUpdatedDescending&skip=0&take={take}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get recently tabled Early Day Motions (EDMs). Use when you want to see the latest political motions or backbench MP initiatives. EDMs are formal motions expressing MP opinions on various issues.")]
        public async Task<string> GetRecentlyTabledEdmsAsync(int take = 10)
        {
            var url = $"{OralQuestionsApiBase}/EarlyDayMotions/list?parameters.orderBy=DateTabledDesc&skip=0&take={take}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Find parliamentary committee meetings scheduled between specific dates. Use when you need to know what committees are meeting or have met in a date range. Includes both House of Commons and House of Lords committees.")]
        public async Task<string> GetCommitteeMeetingsAsync(
            [Description("From date in format YYYY-MM-DD")] string fromdate,
            [Description("To date in format YYYY-MM-DD")] string todate)
        {
            var url = $"{CommitteesApiBase}/Broadcast/Meetings?FromDate={Uri.EscapeDataString(fromdate)}&ToDate={Uri.EscapeDataString(todate)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get list of government departments and bodies that answer parliamentary questions. Use when you need to know which department handles specific policy areas or who answers questions on particular topics.")]
        public async Task<string> GetAnsweringBodiesAsync()
        {
            var url = $"{MembersApiBase}/Reference/AnsweringBodies";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get detailed information about a specific UK Parliament member using their unique ID. Use when you have a member ID and need their full profile, constituency, party, and parliamentary roles.")]
        public async Task<string> GetMemberByIdAsync(int id)
        {
            var url = $"{MembersApiBase}/Members/{id}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for international treaties involving the UK that are scrutinized by Parliament. Use when researching international agreements, trade deals, or diplomatic treaties.")]
        public async Task<string> SearchTreatiesAsync(string searchText)
        {
            var url = $"{TreatiesApiBase}/Treaty?SearchText={Uri.EscapeDataString(searchText)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search Register of Interests for a specific MP or Lord. Use when investigating potential conflicts of interest, financial interests, or external roles of parliament members. ROI shows declared interests like directorships, consultancies, and gifts.")]
        public async Task<string> SearchRoiAsync(int memberId)
        {
            var url = $"{InterestsApiBase}/Interests/?MemberId={memberId}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search Erskine May parliamentary procedure manual. Use when you need to understand parliamentary rules, procedures, or precedents. Erskine May is the authoritative guide to parliamentary procedure.")]
        public async Task<string> SearchErskineMayAsync(string searchTerm)
        {
            var url = $"{ErskineMayApiBase}/Search/ParagraphSearchResults/{Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search House of Commons voting records (divisions). Use when you want to find how MPs voted on specific issues, bills, or amendments. Can filter by member, date range, or division number.")]
        public async Task<string> SearchCommonsDivisionsAsync(string searchTerm, int? memberId = null, string startDate = null, string endDate = null, int? divisionNumber = null)
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

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search House of Lords voting records (divisions). Use when you want to find how Lords voted on specific issues, bills, or amendments in the upper chamber.")]
        public async Task<string> SearchLordsDivisionsAsync(string searchTerm)
        {
            var url = $"{LordsVotesApiBase}/divisions/search?queryParameters.searchTerm={Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for parliamentary bills by title, subject, or keyword. Use when researching proposed legislation, finding bills on specific topics, or tracking legislative progress.")]
        public async Task<string> SearchBillsAsync(string searchTerm, int? memberId = null)
        {
            var url = $"{BillsApiBase}/Bills?SearchTerm={Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for parliamentary committees by name or subject area. Use when you need to find which committee covers a specific policy area or when researching committee work.")]
        public async Task<string> SearchCommitteesAsync(string searchTerm)
        {
            var url = $"{CommitteesApiBase}/Committees?SearchTerm={Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search Early Day Motions by topic or keyword. Use when researching MP opinions on specific issues or finding motions related to particular subjects. EDMs often reflect backbench MP concerns.")]
        public async Task<string> SearchEarlyDayMotionsAsync(string searchTerm)
        {
            var url = $"{OralQuestionsApiBase}/EarlyDayMotions/list?parameters.searchTerm={Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

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

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for Statutory Instruments (secondary legislation) by name. Use when researching government regulations, rules, or orders made under primary legislation. SIs are used to implement or modify laws.")]
        public async Task<string> SearchStatutoryInstrumentsAsync(string name)
        {
            var url = $"{StatutoryInstrumentsApiBase}/StatutoryInstrument?Name={Uri.EscapeDataString(name)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get all Early Day Motions signed by a specific MP. Use when you want to see what issues a particular member has supported or their political priorities through EDM signatures.")]
        public async Task<string> EdmsForMemberIdAsync(int memberid)
        {
            var url = $"{MembersApiBase}/Members/{memberid}/Edms";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get list of active political parties in either House of Commons (1) or House of Lords (2). Use when you need to know current party representation or party structures in Parliament.")]
        public async Task<string> PartiesListByHouseAsync(int house)
        {
            var url = $"{MembersApiBase}/Parties/GetActive/{house}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get categories of interests that MPs and Lords must declare in the Register of Interests. Use when you need to understand what types of financial or other interests parliamentarians must declare.")]
        public async Task<string> InterestsCategoriesAsync()
        {
            var url = $"{InterestsApiBase}/Categories";
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

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get list of all government departments. Use when you need to know the structure of government or which department handles specific policy areas.")]
        public async Task<string> GetDepartmentsAsync()
        {
            var url = $"{MembersApiBase}/Reference/Departments";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get all types of parliamentary committees (e.g., Select Committee, Public Bill Committee, Delegated Legislation Committee). Use when understanding committee structures or finding the right committee type.")]
        public async Task<string> GetCommitteeTypesAsync()
        {
            var url = $"{CommitteesApiBase}/CommitteeType";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get summary of parliamentary contributions (speeches, questions, interventions) made by a specific member. Use when analyzing an MP or Lord's parliamentary activity and participation levels.")]
        public async Task<string> GetContributionsAsync(int memberid)
        {
            var url = $"{MembersApiBase}/Members/{memberid}/ContributionSummary?page=1";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search Hansard (official parliamentary record) for speeches and debates. Use when researching what was said in Parliament on specific topics, by specific members, or in specific time periods. House: 1=Commons, 2=Lords.")]
        public async Task<string> SearchHansardAsync(int house, string startDate, string endDate, string searchTerm)
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

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get scheduled oral question times for ministers in Parliament. Use when you want to know when specific departments will answer questions or when particular topics will be discussed.")]
        public async Task<string> SearchOralQuestionTimesAsync(string answeringDateStart, string answeringDateEnd)
        {
            var url = $"{OralQuestionsApiBase}/oralquestiontimes/list?parameters.answeringDateStart={Uri.EscapeDataString(answeringDateStart)}&parameters.answeringDateEnd={Uri.EscapeDataString(answeringDateEnd)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get list of published Registers of Interests. Use when you need to see all available interest registers or understand the transparency framework for parliamentary interests.")]
        public async Task<string> GetRegistersOfInterestsAsync()
        {
            var url = $"{InterestsApiBase}/Registers";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get list of UK parliamentary constituencies with pagination support. Use when you need constituency information, want to browse all constituencies, or need constituency data for analysis.")]
        public async Task<string> GetConstituenciesAsync(int? skip = null, int? take = null)
        {
            var url = BuildUrl($"{MembersApiBase}/Location/Constituency/Search", new()
            {
                ["skip"] = skip?.ToString(),
                ["take"] = take?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get historical election results for a specific constituency. Use when researching constituency voting patterns, election history, or past electoral outcomes for a particular area.")]
        public async Task<string> GetElectionResultsForConstituencyAsync(int constituencyid)
        {
            var url = $"{MembersApiBase}/Location/Constituency/{constituencyid}/ElectionResults";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get complete voting record of an MP in House of Commons divisions. Use when analyzing how a specific MP votes, their voting patterns, or their stance on particular issues through their voting history.")]
        public async Task<string> GetCommonsVotingRecordForMemberAsync(int memberId)
        {
            var url = $"{CommonsVotesApiBase}/divisions.json/membervoting?queryParameters.memberId={memberId}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get complete voting record of a Lord in House of Lords divisions. Use when analyzing how a specific Lord votes, their voting patterns, or their stance on particular issues through their voting history.")]
        public async Task<string> GetLordsVotingRecordForMemberAsync(int memberId)
        {
            var url = $"{LordsVotesApiBase}/Divisions/membervoting?MemberId={memberId}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for staff interests declared by Lords. Use when investigating potential conflicts of interest related to Lords' staff or understanding transparency requirements for parliamentary staff.")]
        public async Task<string> GetLordsInterestsStaffAsync(string searchterm = "richard")
        {
            var url = $"{MembersApiBase}/LordsInterests/Staff?searchTerm={Uri.EscapeDataString(searchterm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for Acts of Parliament (primary legislation) by name or topic. Use when researching existing laws, finding legislation on specific subjects, or understanding the legal framework on particular issues.")]
        public async Task<string> SearchActsOfParliamentAsync(string name)
        {
            var url = $"{StatutoryInstrumentsApiBase}/ActOfParliament?Name={Uri.EscapeDataString(name)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search parliamentary calendar for upcoming events and business in either chamber. Use when you want to know what's scheduled in Parliament, upcoming debates, or future parliamentary business. House: Commons/Lords.")]
        public async Task<string> SearchCalendar(string house, string startDate, string endDate)
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
        public async Task<string> GetNonSittingDays(string house, string startDate, string endDate)
        {
            var url = BuildUrl($"{WhatsonApiBase}/events/nonsitting.json", new()
            {
                ["queryParameters.house"] = house,
                ["queryParameters.startDate"] = startDate,
                ["queryParameters.endDate"] = endDate
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get the biography of a member of parliament")]
        public async Task<string> GetMembersBiography(int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/Biography";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get the contact information of a member of parliament")]
        public async Task<string> GetMembersContact(int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/Contact";
            return await GetResult(url);
        }

        private static string BuildUrl(string baseUrl, Dictionary<string, string?> parameters)
        {
            var validParams = parameters
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!)}")
                .ToArray();

            return validParams.Length > 0
                ? $"{baseUrl}?{string.Join("&", validParams)}"
                : baseUrl;
        }

        private async Task<string> GetResult(string url)
        {
            for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
            {
                try
                {
                    using var httpClient = httpClientFactory.CreateClient();
                    httpClient.Timeout = HttpTimeout;
                    
                    logger.LogInformation("Making HTTP request to {Url} (attempt {Attempt}/{MaxAttempts})", url, attempt + 1, MaxRetryAttempts);
                    
                    var response = await httpClient.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        logger.LogInformation("Successfully retrieved data from {Url}", url);
                        return JsonSerializer.Serialize(new { url, data });
                    }
                    
                    if (IsTransientFailure(response.StatusCode))
                    {
                        logger.LogWarning("Transient failure for {Url}: {StatusCode}. Attempt {Attempt}/{MaxAttempts}", 
                            url, response.StatusCode, attempt + 1, MaxRetryAttempts);
                        
                        if (attempt < MaxRetryAttempts - 1)
                        {
                            await Task.Delay(RetryDelay * (attempt + 1));
                            continue;
                        }
                    }
                    
                    var errorMessage = $"HTTP request failed with status {response.StatusCode}: {response.ReasonPhrase}";
                    logger.LogError("Final failure for {Url}: {StatusCode}", url, response.StatusCode);
                    return JsonSerializer.Serialize(new { url, error = errorMessage, statusCode = (int)response.StatusCode });
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    logger.LogWarning("Request to {Url} timed out. Attempt {Attempt}/{MaxAttempts}", url, attempt + 1, MaxRetryAttempts);
                    
                    if (attempt < MaxRetryAttempts - 1)
                    {
                        await Task.Delay(RetryDelay * (attempt + 1));
                        continue;
                    }
                    
                    var timeoutError = "Request timed out after multiple attempts";
                    logger.LogError("Request to {Url} timed out after all retry attempts", url);
                    return JsonSerializer.Serialize(new { url, error = timeoutError });
                }
                catch (HttpRequestException ex)
                {
                    logger.LogWarning(ex, "HTTP request exception for {Url}. Attempt {Attempt}/{MaxAttempts}", url, attempt + 1, MaxRetryAttempts);
                    
                    if (attempt < MaxRetryAttempts - 1)
                    {
                        await Task.Delay(RetryDelay * (attempt + 1));
                        continue;
                    }
                    
                    var networkError = $"Network error: {ex.Message}";
                    logger.LogError(ex, "Network error for {Url} after all retry attempts", url);
                    return JsonSerializer.Serialize(new { url, error = networkError });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error for {Url}", url);
                    return JsonSerializer.Serialize(new { url, error = $"Unexpected error: {ex.Message}" });
                }
            }
            
            return JsonSerializer.Serialize(new { url, error = "Maximum retry attempts exceeded" });
        }
        
        private static bool IsTransientFailure(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.RequestTimeout ||
                   statusCode == HttpStatusCode.TooManyRequests ||
                   statusCode == HttpStatusCode.InternalServerError ||
                   statusCode == HttpStatusCode.BadGateway ||
                   statusCode == HttpStatusCode.ServiceUnavailable ||
                   statusCode == HttpStatusCode.GatewayTimeout;
        }

    }
}
