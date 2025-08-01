using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenData.Mcp.Server.Tools;
using System.ComponentModel;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class CommitteesTools(IHttpClientFactory httpClientFactory, ILogger<CommitteesTools> logger) : BaseTools(httpClientFactory, logger)
    {
        protected const string CommitteesApiBase = "https://committees-api.parliament.uk/api";

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Find committee meetings and hearings by date range | committee meetings, parliamentary hearings, committee schedule, committee calendar, when committees meet | Use for finding committee schedules, planning attendance, or researching past committee activity | Returns meeting details including committees, dates, times, and topics | Covers both Commons and Lords committees")]
        public async Task<string> GetCommitteeMeetingsAsync(
            [Description("Start date | Format: YYYY-MM-DD | Example: 2024-01-15")] string fromdate,
            [Description("End date | Format: YYYY-MM-DD | Must be after start date")] string todate)
        {
            var url = $"{CommitteesApiBase}/Broadcast/Meetings?FromDate={Uri.EscapeDataString(fromdate)}&ToDate={Uri.EscapeDataString(todate)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for parliamentary committees by name or subject area. Use when you need to find which committee covers a specific policy area or when researching committee work.")]
        public async Task<string> SearchCommitteesAsync([Description("Search term for committee names or subject areas (e.g. 'Treasury', 'Health', 'Defence')")] string searchTerm)
        {
            var url = $"{CommitteesApiBase}/Committees?SearchTerm={Uri.EscapeDataString(searchTerm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get all types of parliamentary committees (e.g., Select Committee, Public Bill Committee, Delegated Legislation Committee). Use when understanding committee structures or finding the right committee type.")]
        public async Task<string> GetCommitteeTypesAsync()
        {
            var url = $"{CommitteesApiBase}/CommitteeType";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get comprehensive committee profile and membership details | committee information, committee members, committee purpose, parliamentary committee, scrutiny committee | Use for understanding committee roles, finding committee members, or researching committee activities | Returns full committee details including purpose, members, departments scrutinized, and contact information")]
        public async Task<string> GetCommitteeByIdAsync([Description("Committee ID | Required: get from committee search first | Example: 739")] int committeeId, [Description("Include banner images | Default: false | May increase response size")] bool includeBanners = false, [Description("Show only public committees | Default: true | Recommended for general use")] bool showOnWebsiteOnly = true)
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
    }
}
