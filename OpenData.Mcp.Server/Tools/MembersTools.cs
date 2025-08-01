using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenData.Mcp.Server.Tools;
using System.ComponentModel;

namespace OpenData.Mcp.Server
{
    [McpServerToolType]
    public class MembersTools(IHttpClientFactory httpClientFactory, ILogger<MembersTools> logger) : BaseTools(httpClientFactory, logger)
    {
        protected const string MembersApiBase = "https://members-api.parliament.uk/api";

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for MPs and Lords by name with comprehensive member details | find MP, search politician, lookup member, who is, member search, parliamentary representative | Use for identifying members, checking spellings, finding member IDs, or getting basic member information | Returns member profiles with names, parties, constituencies, and current status")]
        public async Task<string> GetMemberByNameAsync(IMcpServer thisServer, [Description("Full or partial name to search for | Examples: 'Boris Johnson', 'Keir Starmer', 'Smith' | Searches current and former members")] string name)
        {
            var url = $"{MembersApiBase}/Members/Search?Name={Uri.EscapeDataString(name)}";
            var content = await GetResult(url);
            return content;
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get government departments and their parliamentary responsibilities | government departments, ministries, answering bodies, policy areas, department structure, who answers questions | Use for understanding government structure, finding responsible departments, or determining who answers questions on specific topics | Returns department names, abbreviations, and policy responsibilities")]
        public async Task<string> GetAnsweringBodiesAsync()
        {
            var url = $"{MembersApiBase}/Reference/AnsweringBodies";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get comprehensive member profile by ID with full parliamentary details | member details, MP profile, member information, parliamentary roles, constituency data | Use when you have a member ID and need complete biographical, political, and contact information | Returns detailed member data including roles, constituency, party, and career information")]
        public async Task<string> GetMemberByIdAsync([Description("Parliament member ID | Required: get from member search first | Example: 1423")] int id)
        {
            var url = $"{MembersApiBase}/Members/{id}";
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

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get list of all government departments. Use when you need to know the structure of government or which department handles specific policy areas.")]
        public async Task<string> GetDepartmentsAsync()
        {
            var url = $"{MembersApiBase}/Reference/Departments";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get summary of parliamentary contributions (speeches, questions, interventions) made by a specific member. Use when analyzing an MP or Lord's parliamentary activity and participation levels.")]
        public async Task<string> GetContributionsAsync([Description("Parliament member ID to get contribution summary for")] int memberid)
        {
            var url = $"{MembersApiBase}/Members/{memberid}/ContributionSummary?page=1";
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

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Search for staff interests declared by Lords. Use when investigating potential conflicts of interest related to Lords' staff or understanding transparency requirements for parliamentary staff.")]
        public async Task<string> GetLordsInterestsStaffAsync([Description("Search term for staff names or interests (default 'richard')")] string searchterm = "richard")
        {
            var url = $"{MembersApiBase}/LordsInterests/Staff?searchTerm={Uri.EscapeDataString(searchterm)}";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get comprehensive member biography and personal history | MP background, life story, career details, education, personal info, political experience | Use for researching member backgrounds, writing profiles, understanding political journey | Returns detailed biographical data including education, career timeline, and political milestones")]
        public async Task<string> GetMembersBiography([Description("Parliament member ID | Required: get from member search first | Returns comprehensive biographical information")] int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/Biography";
            return await GetResult(url);
        }

        [McpServerTool(ReadOnly = true, Idempotent = true, OpenWorld = false), Description("Get member contact details and office information | MP contact, phone number, email, office address, constituency office | Use for contacting members, finding office locations, or getting official contact details | Returns phone numbers, addresses, and official contact information")]
        public async Task<string> GetMembersContact([Description("Parliament member ID | Required: get from member search first | Returns official contact details")] int memberId)
        {
            var url = $"{MembersApiBase}/Members/{memberId}/Contact";
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
    }
}
