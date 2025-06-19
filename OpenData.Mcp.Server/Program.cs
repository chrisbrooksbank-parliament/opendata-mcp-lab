using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.Net.Http; // Added for HttpClient
using System.Threading.Tasks;
using System.Text.Json; // For JSON serialization

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.
    AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddMemoryCache();

// Register HttpClient for dependency injection
builder.Services.AddHttpClient();

var app = builder.Build();

await app.RunAsync();

[McpServerToolType]
public static class UKParliamentOpenDataTool
{

    [McpServerTool, Description("get information on member of parliament by name")]
    public static async Task<string> GetMemberByNameAsync(IServiceProvider serviceProvider, string name)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://members-api.parliament.uk/api/Members/Search?Name={Uri.EscapeDataString(name)}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Gets list of recently updated bills")]
    public static async Task<string> GetRecentlyUpdatedBillsAsync(IServiceProvider serviceProvider, int take = 10)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://bills-api.parliament.uk/api/v1/Bills?SortOrder=DateUpdatedDescending&skip=0&take={take}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Gets list of recently tabled early day motions")]
    public static async Task<string> GetRecentlyTabledEdmsAsync(IServiceProvider serviceProvider, int take = 10)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://oralquestionsandmotions-api.parliament.uk/EarlyDayMotions/list?parameters.orderBy=DateTabledDesc&skip=0&take={take}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Get list of committee meetings between two dates")]
    public static async Task<string> GetCommitteeMeetingsAsync(
        IServiceProvider serviceProvider,
        [Description("From date in format YYYY-MM-DD")] string fromdate,
        [Description("To date in format YYYY-MM-DD")] string todate)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://committees-api.parliament.uk/api/Broadcast/Meetings?FromDate={Uri.EscapeDataString(fromdate)}&ToDate={Uri.EscapeDataString(todate)}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Get list of answering bodies")]
    public static async Task<string> GetAnsweringBodiesAsync(IServiceProvider serviceProvider)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = "https://members-api.parliament.uk/api/Reference/AnsweringBodies";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Get UK Parliament member by id")]
    public static async Task<string> GetMemberByIdAsync(IServiceProvider serviceProvider, int id)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://members-api.parliament.uk/api/Members/{id}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Search UK parliament treaties")]
    public static async Task<string> SearchTreatiesAsync(IServiceProvider serviceProvider, string searchText)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://treaties-api.parliament.uk/api/Treaty?SearchText={Uri.EscapeDataString(searchText)}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Search registered interests (ROI)")]
    public static async Task<string> SearchRoiAsync(IServiceProvider serviceProvider, int memberId)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://interests-api.parliament.uk/api/v1/Interests/?MemberId={memberId}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Search erskine may")]
    public static async Task<string> SearchErskineMayAsync(IServiceProvider serviceProvider, string searchTerm)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://erskinemay-api.parliament.uk/api/Search/ParagraphSearchResults/{Uri.EscapeDataString(searchTerm)}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Search commons divisions")]
    public static async Task<string> SearchCommonsDivisionsAsync(IServiceProvider serviceProvider, string searchTerm, int? memberId = null, string startDate = null, string endDate = null, int? divisionNumber = null)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"http://commonsvotes-api.parliament.uk/data/divisions.json/search?queryParameters.searchTerm={Uri.EscapeDataString(searchTerm)}";
        if (memberId.HasValue) url += $"&memberId={memberId}";
        if (!string.IsNullOrEmpty(startDate)) url += $"&queryParameters.startDate={Uri.EscapeDataString(startDate)}";
        if (!string.IsNullOrEmpty(endDate)) url += $"&queryParameters.endDate={Uri.EscapeDataString(endDate)}";
        if (divisionNumber.HasValue) url += $"&queryParameters.divisionNumber={divisionNumber}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Search lords divisions")]
    public static async Task<string> SearchLordsDivisionsAsync(IServiceProvider serviceProvider, string searchTerm)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"http://lordsvotes-api.parliament.uk/data/divisions/search?queryParameters.searchTerm={Uri.EscapeDataString(searchTerm)}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Search bills")]
    public static async Task<string> SearchBillsAsync(IServiceProvider serviceProvider, string searchTerm, int? memberId = null)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://bills-api.parliament.uk/api/v1/Bills?SearchTerm={Uri.EscapeDataString(searchTerm)}";
        if (memberId.HasValue) url += $"&MemberId={memberId}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Search committees")]
    public static async Task<string> SearchCommitteesAsync(IServiceProvider serviceProvider, string searchTerm)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://committees-api.parliament.uk/api/Committees?SearchTerm={Uri.EscapeDataString(searchTerm)}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Search early day motions")]
    public static async Task<string> SearchEarlyDayMotionsAsync(IServiceProvider serviceProvider, string searchTerm)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://oralquestionsandmotions-api.parliament.uk/EarlyDayMotions/list?parameters.searchTerm={Uri.EscapeDataString(searchTerm)}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("What is happening now in the commons. Annunciator.")]
    public static async Task<string> HappeningNowInCommonsAsync(IServiceProvider serviceProvider)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = "https://now-api.parliament.uk/api/Message/message/CommonsMain/current";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("What is happening now in the lords. Annunciator.")]
    public static async Task<string> HappeningNowInLordsAsync(IServiceProvider serviceProvider)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = "https://now-api.parliament.uk/api/Message/message/LordsMain/current";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Search statutory instruments by name")]
    public static async Task<string> SearchStatutoryInstrumentsAsync(IServiceProvider serviceProvider, string name)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://statutoryinstruments-api.parliament.uk/api/v2/StatutoryInstrument?Name={Uri.EscapeDataString(name)}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Get early day motions for member id")]
    public static async Task<string> EdmsForMemberIdAsync(IServiceProvider serviceProvider, int memberid)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://members-api.parliament.uk/api/Members/{memberid}/Edms";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Get list of parties by house")]
    public static async Task<string> PartiesListByHouseAsync(IServiceProvider serviceProvider, int house)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://members-api.parliament.uk/api/Parties/GetActive/{house}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Get list of categories members can register interests in")]
    public static async Task<string> InterestsCategoriesAsync(IServiceProvider serviceProvider)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = "https://interests-api.parliament.uk/api/v1/Categories";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Get list of bill types")]
    public static async Task<string> BillTypesAsync(IServiceProvider serviceProvider)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = "https://bills-api.parliament.uk/api/v1/BillTypes";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Get list of bill stages")]
    public static async Task<string> BillStagesAsync(IServiceProvider serviceProvider)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = "https://bills-api.parliament.uk/api/v1/Stages";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Get list of departments")]
    public static async Task<string> GetDepartmentsAsync(IServiceProvider serviceProvider)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = "https://members-api.parliament.uk/api/Reference/Departments";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Get list of committee types")]
    public static async Task<string> GetCommitteeTypesAsync(IServiceProvider serviceProvider)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = "https://committees-api.parliament.uk/api/CommitteeType";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Get contributions from a specified member")]
    public static async Task<string> GetContributionsAsync(IServiceProvider serviceProvider, int memberid)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://members-api.parliament.uk/api/Members/{memberid}/ContributionSummary?page=1";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Search hansard for contributions")]
    public static async Task<string> SearchHansardAsync(IServiceProvider serviceProvider, int house, string startDate, string endDate, string searchTerm)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://hansard-api.parliament.uk/search.json?queryParameters.house={house}&queryParameters.startDate={Uri.EscapeDataString(startDate)}&queryParameters.endDate={Uri.EscapeDataString(endDate)}&queryParameters.searchTerm={Uri.EscapeDataString(searchTerm)}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("Get oral question times")]
    public static async Task<string> SearchOralQuestionTimesAsync(IServiceProvider serviceProvider, string answeringDateStart, string answeringDateEnd)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://oralquestionsandmotions-api.parliament.uk/oralquestiontimes/list?parameters.answeringDateStart={Uri.EscapeDataString(answeringDateStart)}&parameters.answeringDateEnd={Uri.EscapeDataString(answeringDateEnd)}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("get published registers of interests")]
    public static async Task<string> GetRegistersOfInterestsAsync(IServiceProvider serviceProvider)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = "https://interests-api.parliament.uk/api/v1/Registers";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("get list of constituencies")]
    public static async Task<string> GetConstituenciesAsync(IServiceProvider serviceProvider, int? skip = null, int? take = null)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = "https://members-api.parliament.uk/api/Location/Constituency/Search?";
        if (skip.HasValue) url += $"skip={skip.Value}&";
        if (take.HasValue) url += $"take={take.Value}";
        var response = await httpClient.GetAsync(url.TrimEnd('&'));
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("get election results for a constituency")]
    public static async Task<string> GetElectionResultsForConstituencyAsync(IServiceProvider serviceProvider, int constituencyid)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://members-api.parliament.uk/api/Location/Constituency/{constituencyid}/ElectionResults";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("get commons voting i.e. division results for member")]
    public static async Task<string> GetCommonsVotingRecordForMemberAsync(IServiceProvider serviceProvider, int memberId)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://commonsvotes-api.parliament.uk/data/divisions.json/membervoting?queryParameters.memberId={memberId}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("get lords voting i.e. division results for member")]
    public static async Task<string> GetLordsVotingRecordForMemberAsync(IServiceProvider serviceProvider, int memberId)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://lordsvotes-api.parliament.uk/data/Divisions/membervoting?MemberId={memberId}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("get lords interests staff")]
    public static async Task<string> GetLordsInterestsStaffAsync(IServiceProvider serviceProvider, string searchterm = "richard")
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://members-api.parliament.uk/api/LordsInterests/Staff?searchTerm={Uri.EscapeDataString(searchterm)}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    [McpServerTool, Description("search acts of parliament")]
    public static async Task<string> SearchActsOfParliamentAsync(IServiceProvider serviceProvider, string name)
    {
        var httpClient = GetHttpClient(serviceProvider);
        var url = $"https://statutoryinstruments-api.parliament.uk/api/v2/ActOfParliament?Name={Uri.EscapeDataString(name)}";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return BuildResult(url, json);
    }

    private static HttpClient GetHttpClient(IServiceProvider serviceProvider)
    {
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        return httpClientFactory.CreateClient();
    }

    private static string BuildResult(string apiSourceUrl, string data)
    {
        return JsonSerializer.Serialize(new { apiSourceUrl, data });
    }
}