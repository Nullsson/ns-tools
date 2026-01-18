using System.Text.Json;
using FetchPrograms.Model.Intigriti;

namespace FetchPrograms;

public class IntigritiAPIClient : IDisposable
{
    private readonly HttpClient  _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public IntigritiAPIClient(string apiKey)
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://api.intigriti.com/"),
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Nullsson Fetcher");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Get all programs you have access to
    /// </summary>
    public async Task<PaginationViewModel<ProgramOverviewViewModel>> GetProgramsAsync(
        int? statusId = null, int? typeId = null, bool? following = null,
        int? limit = null, int? offset = null, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("external/researcher/v1/programs", new Dictionary<string, string?>
        {
            ["statusId"] = statusId?.ToString(),
            ["typeId"] = typeId?.ToString(),
            ["following"] = following?.ToString().ToLower(),
            ["limit"] = limit?.ToString(),
            ["offset"] = offset?.ToString()
        });

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginationViewModel<ProgramOverviewViewModel>>(content, _jsonOptions)
               ?? new PaginationViewModel<ProgramOverviewViewModel>();
    }

    /// <summary>
    /// Get program details by ID
    /// </summary>
    public async Task<ProgramDetailViewModel> GetProgramDetailAsync(
        Guid programId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"external/researcher/v1/program/{programId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ProgramDetailViewModel>(content, _jsonOptions)
            ?? throw new Exception("Unable to deserialize program detail");
    }

    public async Task<PaginationViewModel<ProgramActivityViewModel>> GetProgramActivitiesAsync(
        long? createdSince = null,
        bool? following = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("external/researcher/v1/programs/activities", new Dictionary<string, string?>
        {
            ["createdSince"] = createdSince?.ToString(),
            ["following"] = following?.ToString().ToLower(),
            ["limit"] = limit?.ToString(),
            ["offset"] = offset?.ToString()
        });
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginationViewModel<ProgramActivityViewModel>>(content, _jsonOptions) 
            ?? new PaginationViewModel<ProgramActivityViewModel>();
    }

    public async Task<ProgramDomainsViewModel> GetProgramDomainsAsync(
        Guid programId,
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"external/researcher/v1/programs/{programId}/domains/{versionId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content =  await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ProgramDomainsViewModel>(content, _jsonOptions)
               ?? new ProgramDomainsViewModel();
    }

    public async Task<ProgramRulesOfEngagementViewModel> GetProgramRulesOfEngagementAsync(
        Guid programId,
        Guid versionId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"external/researcher/v1/programs/{programId}/rules-of-engagements/{versionId}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ProgramRulesOfEngagementViewModel>(content, _jsonOptions)
               ?? throw new InvalidOperationException("Failed to deserialize rules of engagement");
    }

    public async Task<PaginationViewModel<PayoutOverviewViewModel>> GetPayoutsAsync(
        int? statusId = null,
        long? createdSince = null,
        long? paidSince = null,
        int? limit = null,
        int? offset = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("external/researcher/v1/payouts", new Dictionary<string, string?>
        {
            ["statusId"] = statusId?.ToString(),
            ["createdSince"] = createdSince?.ToString(),
            ["paidSince"] = paidSince?.ToString(),
            ["limit"] = limit?.ToString(),
            ["offset"] = offset?.ToString()
        });
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginationViewModel<PayoutOverviewViewModel>>(content, _jsonOptions)
            ??  new PaginationViewModel<PayoutOverviewViewModel>();
    }
    
    private static string BuildUrl(string endpoint, Dictionary<string, string?> parameters)
    {
        var queryParams = parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}");

        var queryString = string.Join("&", queryParams);
        return string.IsNullOrEmpty(queryString) ? endpoint : $"{endpoint}?{queryString}";
    }
    
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}