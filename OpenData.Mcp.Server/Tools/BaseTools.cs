using System.Text.Json;
using System.Net;
using Microsoft.Extensions.Logging;

namespace OpenData.Mcp.Server.Tools
{
    /// <summary>
    /// Base class for Parliament API tools providing common functionality for HTTP operations,
    /// retry logic, URL building, and error handling.
    /// </summary>
    public abstract class BaseTools
    {
        protected readonly IHttpClientFactory HttpClientFactory;
        protected readonly ILogger Logger;

        // HTTP configuration constants
        protected static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(30);
        protected const int MaxRetryAttempts = 3;
        protected static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(1);

        protected BaseTools(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            HttpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Builds a URL with query parameters, filtering out null or empty values.
        /// </summary>
        /// <param name="baseUrl">The base URL</param>
        /// <param name="parameters">Dictionary of parameter key-value pairs</param>
        /// <returns>Complete URL with query string</returns>
        protected static string BuildUrl(string baseUrl, Dictionary<string, string?> parameters)
        {
            var validParams = parameters
                .Where(kvp => !string.IsNullOrEmpty(kvp.Value))
                .Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value!)}")
                .ToArray();

            return validParams.Length > 0
                ? $"{baseUrl}?{string.Join("&", validParams)}"
                : baseUrl;
        }

        /// <summary>
        /// Makes an HTTP GET request with retry logic and comprehensive error handling.
        /// Returns JSON serialized response with URL and data/error information.
        /// </summary>
        /// <param name="url">The URL to make the request to</param>
        /// <returns>JSON serialized response containing URL and either data or error details</returns>
        protected async Task<string> GetResult(string url)
        {
            for (int attempt = 0; attempt < MaxRetryAttempts; attempt++)
            {
                try
                {
                    using var httpClient = HttpClientFactory.CreateClient();
                    httpClient.Timeout = HttpTimeout;
                    
                    Logger.LogInformation("Making HTTP request to {Url} (attempt {Attempt}/{MaxAttempts})", 
                        url, attempt + 1, MaxRetryAttempts);
                    
                    var response = await httpClient.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        Logger.LogInformation("Successfully retrieved data from {Url}", url);
                        return JsonSerializer.Serialize(new { url, data });
                    }
                    
                    if (IsTransientFailure(response.StatusCode))
                    {
                        Logger.LogWarning("Transient failure for {Url}: {StatusCode}. Attempt {Attempt}/{MaxAttempts}", 
                            url, response.StatusCode, attempt + 1, MaxRetryAttempts);
                        
                        if (attempt < MaxRetryAttempts - 1)
                        {
                            await Task.Delay(RetryDelay * (attempt + 1));
                            continue;
                        }
                    }
                    
                    var errorMessage = $"HTTP request failed with status {response.StatusCode}: {response.ReasonPhrase}";
                    Logger.LogError("Final failure for {Url}: {StatusCode}", url, response.StatusCode);
                    return JsonSerializer.Serialize(new { url, error = errorMessage, statusCode = (int)response.StatusCode });
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    Logger.LogWarning("Request to {Url} timed out. Attempt {Attempt}/{MaxAttempts}", 
                        url, attempt + 1, MaxRetryAttempts);
                    
                    if (attempt < MaxRetryAttempts - 1)
                    {
                        await Task.Delay(RetryDelay * (attempt + 1));
                        continue;
                    }
                    
                    var timeoutError = "Request timed out after multiple attempts";
                    Logger.LogError("Request to {Url} timed out after all retry attempts", url);
                    return JsonSerializer.Serialize(new { url, error = timeoutError });
                }
                catch (HttpRequestException ex)
                {
                    Logger.LogWarning(ex, "HTTP request exception for {Url}. Attempt {Attempt}/{MaxAttempts}", 
                        url, attempt + 1, MaxRetryAttempts);
                    
                    if (attempt < MaxRetryAttempts - 1)
                    {
                        await Task.Delay(RetryDelay * (attempt + 1));
                        continue;
                    }
                    
                    var networkError = $"Network error: {ex.Message}";
                    Logger.LogError(ex, "Network error for {Url} after all retry attempts", url);
                    return JsonSerializer.Serialize(new { url, error = networkError });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Unexpected error for {Url}", url);
                    return JsonSerializer.Serialize(new { url, error = $"Unexpected error: {ex.Message}" });
                }
            }
            
            return JsonSerializer.Serialize(new { url, error = "Maximum retry attempts exceeded" });
        }
        
        /// <summary>
        /// Determines if an HTTP status code represents a transient failure that should be retried.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to check</param>
        /// <returns>True if the failure is transient and should be retried</returns>
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