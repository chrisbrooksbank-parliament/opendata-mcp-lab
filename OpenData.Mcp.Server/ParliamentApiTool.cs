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
        public async Task<string> GetMemberByNameAsync(IMcpServer thisServer, [Description("Full or partial name of the MP or Lord to search for (e.g. 'Boris Johnson', 'Keir Starmer')")] string name)
        {
            var url = $"{MembersApiBase}/Members/Search?Name={Uri.EscapeDataString(name)}";
            var content = await GetResult(url);
            return content;
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get the most recently updated bills in Parliament. Use when you want current legislative activity or recent changes to proposed laws. Returns bill titles, stages, sponsors, and last update dates.")]
        public async Task<string> GetRecentlyUpdatedBillsAsync([Description("Number of bills to return (default 10, max recommended 50)")] int take = 10)
        {
            var url = $"{BillsApiBase}/Bills?SortOrder=DateUpdatedDescending&skip=0&take={take}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get recently tabled Early Day Motions (EDMs). Use when you want to see the latest political motions or backbench MP initiatives. EDMs are formal motions expressing MP opinions on various issues.")]
        public async Task<string> GetRecentlyTabledEdmsAsync([Description("Number of Early Day Motions to return (default 10, max recommended 50)")] int take = 10)
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
        public async Task<string> GetMemberByIdAsync([Description("Unique Parliament member ID number")] int id)
        {
            var url = $"{MembersApiBase}/Members/{id}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for international treaties involving the UK that are scrutinized by Parliament. Use when researching international agreements, trade deals, or diplomatic treaties.")]
        public async Task<string> SearchTreatiesAsync([Description("Search term for treaty titles or content (e.g. 'trade', 'EU', 'climate')")] string searchText)
        {
            var url = $"{TreatiesApiBase}/Treaty?SearchText={Uri.EscapeDataString(searchText)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search Register of Interests for a specific MP or Lord. Use when investigating potential conflicts of interest, financial interests, or external roles of parliament members. ROI shows declared interests like directorships, consultancies, and gifts.")]
        public async Task<string> SearchRoiAsync([Description("Parliament member ID to search interests for")] int memberId)
        {
            var url = $"{InterestsApiBase}/Interests/?MemberId={memberId}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search Erskine May parliamentary procedure manual. Use when you need to understand parliamentary rules, procedures, or precedents. Erskine May is the authoritative guide to parliamentary procedure.")]
        public async Task<string> SearchErskineMayAsync([Description("Search term for parliamentary procedure rules (e.g. 'Speaker', 'amendment', 'division')")] string searchTerm)
        {
            var url = $"{ErskineMayApiBase}/Search/ParagraphSearchResults/{Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

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

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search House of Lords voting records (divisions). Use when you want to find how Lords voted on specific issues, bills, or amendments in the upper chamber.")]
        public async Task<string> SearchLordsDivisionsAsync([Description("Search term for Lords division topics (e.g. 'brexit', 'climate', 'NHS')")] string searchTerm)
        {
            var url = $"{LordsVotesApiBase}/divisions/search?queryParameters.searchTerm={Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for parliamentary bills by title, subject, or keyword. Use when researching proposed legislation, finding bills on specific topics, or tracking legislative progress.")]
        public async Task<string> SearchBillsAsync([Description("Search term for bill titles or content (e.g. 'environment', 'health', 'finance')")] string searchTerm, [Description("Optional: member ID to filter bills sponsored by specific member")] int? memberId = null)
        {
            var url = $"{BillsApiBase}/Bills?SearchTerm={Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for parliamentary committees by name or subject area. Use when you need to find which committee covers a specific policy area or when researching committee work.")]
        public async Task<string> SearchCommitteesAsync([Description("Search term for committee names or subject areas (e.g. 'Treasury', 'Health', 'Defence')")] string searchTerm)
        {
            var url = $"{CommitteesApiBase}/Committees?SearchTerm={Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search Early Day Motions by topic or keyword. Use when researching MP opinions on specific issues or finding motions related to particular subjects. EDMs often reflect backbench MP concerns.")]
        public async Task<string> SearchEarlyDayMotionsAsync([Description("Search term for EDM topics or content (e.g. 'climate change', 'NHS funding')")] string searchTerm)
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
        public async Task<string> SearchStatutoryInstrumentsAsync([Description("Name or title of the statutory instrument to search for")] string name)
        {
            var url = $"{StatutoryInstrumentsApiBase}/StatutoryInstrument?Name={Uri.EscapeDataString(name)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get all Early Day Motions signed by a specific MP. Use when you want to see what issues a particular member has supported or their political priorities through EDM signatures.")]
        public async Task<string> EdmsForMemberIdAsync([Description("Parliament member ID to get EDMs for")] int memberid)
        {
            var url = $"{MembersApiBase}/Members/{memberid}/Edms";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get list of active political parties in either House of Commons (1) or House of Lords (2). Use when you need to know current party representation or party structures in Parliament.")]
        public async Task<string> PartiesListByHouseAsync([Description("House number: 1 for Commons, 2 for Lords")] int house)
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
        public async Task<string> GetContributionsAsync([Description("Parliament member ID to get contribution summary for")] int memberid)
        {
            var url = $"{MembersApiBase}/Members/{memberid}/ContributionSummary?page=1";
            return await GetResult(url);
        }

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

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get scheduled oral question times for ministers in Parliament. Use when you want to know when specific departments will answer questions or when particular topics will be discussed.")]
        public async Task<string> SearchOralQuestionTimesAsync([Description("Start date for question times in YYYY-MM-DD format")] string answeringDateStart, [Description("End date for question times in YYYY-MM-DD format")] string answeringDateEnd)
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
        public async Task<string> GetConstituenciesAsync([Description("Number of constituencies to skip (for pagination)")] int? skip = null, [Description("Number of constituencies to return (default 20, max 100)")] int? take = null)
        {
            var url = BuildUrl($"{MembersApiBase}/Location/Constituency/Search", new()
            {
                ["skip"] = skip?.ToString(),
                ["take"] = take?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get historical election results for a specific constituency. Use when researching constituency voting patterns, election history, or past electoral outcomes for a particular area.")]
        public async Task<string> GetElectionResultsForConstituencyAsync([Description("Unique constituency ID number")] int constituencyid)
        {
            var url = $"{MembersApiBase}/Location/Constituency/{constituencyid}/ElectionResults";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get complete voting record of an MP in House of Commons divisions. Use when analyzing how a specific MP votes, their voting patterns, or their stance on particular issues through their voting history.")]
        public async Task<string> GetCommonsVotingRecordForMemberAsync([Description("Parliament member ID to get Commons voting record for")] int memberId)
        {
            var url = $"{CommonsVotesApiBase}/divisions.json/membervoting?queryParameters.memberId={memberId}";
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

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for staff interests declared by Lords. Use when investigating potential conflicts of interest related to Lords' staff or understanding transparency requirements for parliamentary staff.")]
        public async Task<string> GetLordsInterestsStaffAsync([Description("Search term for staff names or interests (default 'richard')")] string searchterm = "richard")
        {
            var url = $"{MembersApiBase}/LordsInterests/Staff?searchTerm={Uri.EscapeDataString(searchterm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for Acts of Parliament (primary legislation) by name or topic. Use when researching existing laws, finding legislation on specific subjects, or understanding the legal framework on particular issues.")]
        public async Task<string> SearchActsOfParliamentAsync([Description("Name or title of the Act to search for (e.g. 'Climate Change Act', 'Human Rights Act')")] string name)
        {
            var url = $"{StatutoryInstrumentsApiBase}/ActOfParliament?Name={Uri.EscapeDataString(name)}";
            return await GetResult(url);
        }

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

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get the biography of a member of parliament")]
        public async Task<string> GetMembersBiography([Description("Parliament member ID to get biography for")] int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/Biography";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get the contact information of a member of parliament")]
        public async Task<string> GetMembersContact([Description("Parliament member ID to get contact details for")] int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/Contact";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get detailed information about a specific committee by ID. Use when you need complete committee details including members, purpose, scrutinising departments, and contact information.")]
        public async Task<string> GetCommitteeByIdAsync([Description("Unique committee ID number")] int committeeId, [Description("Include banner images in response")] bool includeBanners = false, [Description("Show only committees visible on website")] bool showOnWebsiteOnly = true)
        {
            var url = BuildUrl($"{CommitteesApiBase}/Committees/{committeeId}", new()
            {
                ["includeBanners"] = includeBanners.ToString(),
                ["showOnWebsiteOnly"] = showOnWebsiteOnly.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for committee events with flexible filtering options. Use when you need to find meetings, hearings, or other committee activities by date, location, or committee. Supports filtering by start/end dates, house, event type, and location.")]
        public async Task<string> GetEventsAsync([Description("Optional: filter by specific committee ID")] int? committeeId = null, [Description("Optional: filter by committee business ID")] int? committeeBusinessId = null, [Description("Optional: search term for event titles or content")] string searchTerm = null, [Description("Optional: start date from in YYYY-MM-DD format")] string startDateFrom = null, [Description("Optional: start date to in YYYY-MM-DD format")] string startDateTo = null, [Description("Optional: end date from in YYYY-MM-DD format")] string endDateFrom = null, [Description("Optional: location ID to filter events")] int? locationId = null, [Description("Optional: exclude cancelled events")] bool? excludeCancelledEvents = null, [Description("Optional: sort ascending by date")] bool? sortAscending = null, [Description("Optional: filter by event type ID")] int? eventTypeId = null, [Description("Include event attendees in response")] bool includeEventAttendees = false, [Description("Show only events visible on website")] bool showOnWebsiteOnly = true, [Description("Number of records to skip (for pagination)")] int skip = 0, [Description("Number of records to return (default 30, max 100)")] int take = 30)
        {
            var url = BuildUrl($"{CommitteesApiBase}/Events", new()
            {
                ["CommitteeId"] = committeeId?.ToString(),
                ["CommitteeBusinessId"] = committeeBusinessId?.ToString(),
                ["SearchTerm"] = searchTerm,
                ["StartDateFrom"] = startDateFrom,
                ["StartDateTo"] = startDateTo,
                ["EndDateFrom"] = endDateFrom,
                ["LocationId"] = locationId?.ToString(),
                ["ExcludeCancelledEvents"] = excludeCancelledEvents?.ToString(),
                ["SortAscending"] = sortAscending?.ToString(),
                ["EventTypeId"] = eventTypeId?.ToString(),
                ["IncludeEventAttendees"] = includeEventAttendees.ToString(),
                ["ShowOnWebsiteOnly"] = showOnWebsiteOnly.ToString(),
                ["Skip"] = skip.ToString(),
                ["Take"] = take.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get detailed information about a specific committee event by ID. Use when you need complete event details including activities, attendees, committees involved, and related business.")]
        public async Task<string> GetEventByIdAsync([Description("Unique event ID number")] int eventId, [Description("Show only events visible on website")] bool showOnWebsiteOnly = true)
        {
            var url = BuildUrl($"{CommitteesApiBase}/Events/{eventId}", new()
            {
                ["showOnWebsiteOnly"] = showOnWebsiteOnly.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get events for a specific committee by committee ID. Use when you want to see all meetings and activities for a particular committee, with options to filter by date range, business, and event type.")]
        public async Task<string> GetCommitteeEventsAsync([Description("Committee ID to get events for")] int committeeId, [Description("Optional: filter by committee business ID")] int? committeeBusinessId = null, [Description("Optional: search term for event titles or content")] string searchTerm = null, [Description("Optional: start date from in YYYY-MM-DD format")] string startDateFrom = null, [Description("Optional: start date to in YYYY-MM-DD format")] string startDateTo = null, [Description("Optional: end date from in YYYY-MM-DD format")] string endDateFrom = null, [Description("Optional: location ID to filter events")] int? locationId = null, [Description("Optional: exclude cancelled events")] bool? excludeCancelledEvents = null, [Description("Optional: sort ascending by date")] bool? sortAscending = null, [Description("Optional: filter by event type ID")] int? eventTypeId = null, [Description("Include event attendees in response")] bool includeEventAttendees = false, [Description("Show only events visible on website")] bool showOnWebsiteOnly = true, [Description("Number of records to skip (for pagination)")] int skip = 0, [Description("Number of records to return (default 30, max 100)")] int take = 30)
        {
            var url = BuildUrl($"{CommitteesApiBase}/Committees/{committeeId}/Events", new()
            {
                ["CommitteeBusinessId"] = committeeBusinessId?.ToString(),
                ["SearchTerm"] = searchTerm,
                ["StartDateFrom"] = startDateFrom,
                ["StartDateTo"] = startDateTo,
                ["EndDateFrom"] = endDateFrom,
                ["LocationId"] = locationId?.ToString(),
                ["ExcludeCancelledEvents"] = excludeCancelledEvents?.ToString(),
                ["SortAscending"] = sortAscending?.ToString(),
                ["EventTypeId"] = eventTypeId?.ToString(),
                ["IncludeEventAttendees"] = includeEventAttendees.ToString(),
                ["ShowOnWebsiteOnly"] = showOnWebsiteOnly.ToString(),
                ["Skip"] = skip.ToString(),
                ["Take"] = take.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get members and staff of a specific committee by committee ID. Use when you need to know who serves on a committee, their roles, and membership status (current/former). Returns both elected members and lay members.")]
        public async Task<string> GetCommitteeMembersAsync([Description("Committee ID to get members for")] int committeeId, [Description("Optional: filter by membership status (e.g. 'Current', 'Former')")] string membershipStatus = null, [Description("Show only members visible on website")] bool showOnWebsiteOnly = true, [Description("Number of records to skip (for pagination)")] int skip = 0, [Description("Number of records to return (default 30, max 100)")] int take = 30)
        {
            var url = BuildUrl($"{CommitteesApiBase}/Committees/{committeeId}/Members", new()
            {
                ["MembershipStatus"] = membershipStatus,
                ["ShowOnWebsiteOnly"] = showOnWebsiteOnly.ToString(),
                ["Skip"] = skip.ToString(),
                ["Take"] = take.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for committee publications including reports, government responses, and other documents. Use when researching committee outputs, finding reports on specific topics, or tracking publication dates and paper numbers.")]
        public async Task<string> GetPublicationsAsync([Description("Optional: array of publication type IDs to filter by")] int[] publicationTypeIds = null, [Description("Optional: search term for publication titles or content")] string searchTerm = null, [Description("Optional: start date in YYYY-MM-DD format")] string startDate = null, [Description("Optional: end date in YYYY-MM-DD format")] string endDate = null, [Description("Optional: array of paper numbers to filter by")] string[] paperNumbers = null, [Description("Optional: committee business ID to filter by")] int? committeeBusinessId = null, [Description("Optional: committee ID to filter by")] int? committeeId = null, [Description("Show only publications visible on website")] bool showOnWebsiteOnly = true, [Description("Number of records to skip (for pagination)")] int skip = 0, [Description("Number of records to return (default 30, max 100)")] int take = 30)
        {
            var url = BuildUrl($"{CommitteesApiBase}/Publications", new()
            {
                ["SearchTerm"] = searchTerm,
                ["StartDate"] = startDate,
                ["EndDate"] = endDate,
                ["CommitteeBusinessId"] = committeeBusinessId?.ToString(),
                ["CommitteeId"] = committeeId?.ToString(),
                ["ShowOnWebsiteOnly"] = showOnWebsiteOnly.ToString(),
                ["Skip"] = skip.ToString(),
                ["Take"] = take.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get detailed information about a specific committee publication by ID. Use when you need complete publication details including documents, HC numbers, government responses, and associated committee business.")]
        public async Task<string> GetPublicationByIdAsync([Description("Unique publication ID number")] int publicationId, [Description("Show only publications visible on website")] bool showOnWebsiteOnly = true)
        {
            var url = BuildUrl($"{CommitteesApiBase}/Publications/{publicationId}", new()
            {
                ["showOnWebsiteOnly"] = showOnWebsiteOnly.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for written evidence submissions to committees. Use when researching stakeholder submissions, witness statements, or public input to committee inquiries. Can filter by committee, business, witness names, or publication dates.")]
        public async Task<string> GetWrittenEvidenceAsync([Description("Optional: committee business ID to filter by")] int? committeeBusinessId = null, [Description("Optional: committee ID to filter by")] int? committeeId = null, [Description("Optional: search term for evidence content or witness names")] string searchTerm = null, [Description("Optional: start date in YYYY-MM-DD format")] string startDate = null, [Description("Optional: end date in YYYY-MM-DD format")] string endDate = null, [Description("Show only evidence visible on website")] bool showOnWebsiteOnly = true, [Description("Number of records to skip (for pagination)")] int skip = 0, [Description("Number of records to return (default 30, max 100)")] int take = 30)
        {
            var url = BuildUrl($"{CommitteesApiBase}/WrittenEvidence", new()
            {
                ["CommitteeBusinessId"] = committeeBusinessId?.ToString(),
                ["CommitteeId"] = committeeId?.ToString(),
                ["SearchTerm"] = searchTerm,
                ["StartDate"] = startDate,
                ["EndDate"] = endDate,
                ["ShowOnWebsiteOnly"] = showOnWebsiteOnly.ToString(),
                ["Skip"] = skip.ToString(),
                ["Take"] = take.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for oral evidence sessions from committee hearings. Use when researching witness testimonies, committee hearings, or transcripts from evidence sessions. Can filter by committee, business, witness names, or meeting dates.")]
        public async Task<string> GetOralEvidenceAsync([Description("Optional: committee business ID to filter by")] int? committeeBusinessId = null, [Description("Optional: committee ID to filter by")] int? committeeId = null, [Description("Optional: search term for evidence content or witness names")] string searchTerm = null, [Description("Optional: start date in YYYY-MM-DD format")] string startDate = null, [Description("Optional: end date in YYYY-MM-DD format")] string endDate = null, [Description("Show only evidence visible on website")] bool showOnWebsiteOnly = true, [Description("Number of records to skip (for pagination)")] int skip = 0, [Description("Number of records to return (default 30, max 100)")] int take = 30)
        {
            var url = BuildUrl($"{CommitteesApiBase}/OralEvidence", new()
            {
                ["CommitteeBusinessId"] = committeeBusinessId?.ToString(),
                ["CommitteeId"] = committeeId?.ToString(),
                ["SearchTerm"] = searchTerm,
                ["StartDate"] = startDate,
                ["EndDate"] = endDate,
                ["ShowOnWebsiteOnly"] = showOnWebsiteOnly.ToString(),
                ["Skip"] = skip.ToString(),
                ["Take"] = take.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for current MPs and Lords with comprehensive filtering options. Use when you need to find members by name, location, party, constituency, gender, posts held, or policy interests. Supports advanced search criteria including membership dates and eligibility status.")]
        public async Task<string> SearchMembersAsync([Description("Optional: full or partial name to search for")] string name = null, [Description("Optional: location or constituency name")] string location = null, [Description("Optional: post title (e.g. 'Minister', 'Secretary of State')")] string postTitle = null, [Description("Optional: party ID to filter by")] int? partyId = null, [Description("Optional: house number (1=Commons, 2=Lords)")] int? house = null, [Description("Optional: constituency ID to filter by")] int? constituencyId = null, [Description("Optional: filter names starting with specific letter(s)")] string nameStartsWith = null, [Description("Optional: gender filter ('M' or 'F')")] string gender = null, [Description("Optional: membership started since date in YYYY-MM-DD format")] string membershipStartedSince = null, [Description("Optional: membership ended since date in YYYY-MM-DD format")] string membershipEndedSince = null, [Description("Optional: was member on or after date in YYYY-MM-DD format")] string wasMemberOnOrAfter = null, [Description("Optional: was member on or before date in YYYY-MM-DD format")] string wasMemberOnOrBefore = null, [Description("Optional: was member of house (1=Commons, 2=Lords)")] int? wasMemberOfHouse = null, [Description("Optional: filter by eligibility status")] bool? isEligible = null, [Description("Optional: filter by current membership status")] bool? isCurrentMember = null, [Description("Optional: policy interest ID to filter by")] int? policyInterestId = null, [Description("Optional: search term for professional experience")] string experience = null, [Description("Number of records to skip (for pagination)")] int skip = 0, [Description("Number of records to return (default 20, max 100)")] int take = 20)
        {
            var url = BuildUrl($"{MembersApiBase}/Members/Search", new()
            {
                ["Name"] = name,
                ["Location"] = location,
                ["PostTitle"] = postTitle,
                ["PartyId"] = partyId?.ToString(),
                ["House"] = house?.ToString(),
                ["ConstituencyId"] = constituencyId?.ToString(),
                ["NameStartsWith"] = nameStartsWith,
                ["Gender"] = gender,
                ["MembershipStartedSince"] = membershipStartedSince,
                ["MembershipEnded.MembershipEndedSince"] = membershipEndedSince,
                ["MembershipInDateRange.WasMemberOnOrAfter"] = wasMemberOnOrAfter,
                ["MembershipInDateRange.WasMemberOnOrBefore"] = wasMemberOnOrBefore,
                ["MembershipInDateRange.WasMemberOfHouse"] = wasMemberOfHouse?.ToString(),
                ["IsEligible"] = isEligible?.ToString(),
                ["IsCurrentMember"] = isCurrentMember?.ToString(),
                ["PolicyInterestId"] = policyInterestId?.ToString(),
                ["Experience"] = experience,
                ["skip"] = skip.ToString(),
                ["take"] = take.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for historical members who were active on a specific date. Use when you need to find MPs or Lords who served during a particular period in parliamentary history.")]
        public async Task<string> SearchMembersHistoricalAsync([Description("Optional: full or partial name to search for")] string name = null, [Description("Optional: specific date to search for members active on that date (YYYY-MM-DD format)")] string dateToSearchFor = null, [Description("Number of records to skip (for pagination)")] int skip = 0, [Description("Number of records to return (default 20, max 100)")] int take = 20)
        {
            var url = BuildUrl($"{MembersApiBase}/Members/SearchHistorical", new()
            {
                ["name"] = name,
                ["dateToSearchFor"] = dateToSearchFor,
                ["skip"] = skip.ToString(),
                ["take"] = take.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get professional experience and career background of a member by ID. Use when researching a member's qualifications, previous employment, education, or professional history before entering Parliament.")]
        public async Task<string> GetMemberExperienceAsync([Description("Parliament member ID to get professional experience for")] int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/Experience";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get areas of focus and policy interests of a member by ID. Use when understanding what issues and policy areas a member prioritizes or specializes in.")]
        public async Task<string> GetMemberFocusAsync([Description("Parliament member ID to get policy focus areas for")] int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/Focus";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get registered interests of a member by ID and house. Use when investigating potential conflicts of interest, financial interests, or external roles. Shows declared interests like directorships, consultancies, and gifts.")]
        public async Task<string> GetMemberRegisteredInterestsAsync([Description("Parliament member ID to get registered interests for")] int memberId, [Description("Optional: house number (1=Commons, 2=Lords)")] int? house = null)
        {
            var url = BuildUrl($"{MembersApiBase}/Members/{memberId}/RegisteredInterests", new()
            {
                ["house"] = house?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get staff members working for a specific MP or Lord by member ID. Use when researching parliamentary office staff, researchers, or support personnel.")]
        public async Task<string> GetMemberStaffAsync([Description("Parliament member ID to get staff details for")] int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/Staff";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get a brief synopsis or summary about a member by ID. Use when you need a concise overview of a member's background, role, or key information.")]
        public async Task<string> GetMemberSynopsisAsync([Description("Parliament member ID to get synopsis for")] int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/Synopsis";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get voting records of a member by ID for a specific house. Use when analyzing how a member votes, their voting patterns, or their stance on particular issues through their voting history.")]
        public async Task<string> GetMemberVotingAsync([Description("Parliament member ID to get voting record for")] int memberId, [Description("House number (1=Commons, 2=Lords)")] int house, [Description("Optional: page number for pagination")] int? page = null)
        {
            var url = BuildUrl($"{MembersApiBase}/Members/{memberId}/Voting", new()
            {
                ["house"] = house.ToString(),
                ["page"] = page?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get written questions submitted by a member by ID. Use when researching what questions a member has asked of government departments or their areas of parliamentary inquiry.")]
        public async Task<string> GetMemberWrittenQuestionsAsync([Description("Parliament member ID to get written questions for")] int memberId, [Description("Optional: page number for pagination")] int? page = null)
        {
            var url = BuildUrl($"{MembersApiBase}/Members/{memberId}/WrittenQuestions", new()
            {
                ["page"] = page?.ToString()
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get historical information for multiple members by their IDs. Returns name history, party affiliations, and membership details over time. Use when researching how members' details have changed throughout their careers.")]
        public async Task<string> GetMembersHistoryAsync([Description("Array of Parliament member IDs to get history for")] int[] memberIds)
        {
            var url = BuildUrl($"{MembersApiBase}/Members/History", new()
            {
                ["ids"] = string.Join(",", memberIds)
            });
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get the latest election result for a member by ID. Use when researching how a member was elected, their constituency performance, vote share, or election margin.")]
        public async Task<string> GetMemberLatestElectionResultAsync([Description("Parliament member ID to get latest election result for")] int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/LatestElectionResult";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get the portrait image URL for a member by ID. Use when you need a link to a member's official parliamentary portrait photograph.")]
        public async Task<string> GetMemberPortraitUrlAsync([Description("Parliament member ID to get portrait URL for")] int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/PortraitUrl";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get the thumbnail image URL for a member by ID. Use when you need a link to a smaller version of a member's parliamentary photograph for lists or compact displays.")]
        public async Task<string> GetMemberThumbnailUrlAsync([Description("Parliament member ID to get thumbnail URL for")] int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/ThumbnailUrl";
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
