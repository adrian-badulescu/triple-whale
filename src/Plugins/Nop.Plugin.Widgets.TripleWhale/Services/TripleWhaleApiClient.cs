using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Nop.Plugin.Widgets.TripleWhale.Models.Dto;

namespace Nop.Plugin.Widgets.TripleWhale.Services;

/// <summary>
/// HTTP client for the Triple Whale API with retry, exponential backoff and rate-limit handling
/// </summary>
public class TripleWhaleApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TripleWhaleSettings _settings;

    /// <summary>
    /// Shared JSON options: field names are mapped via [JsonPropertyName], null values are omitted
    /// </summary>
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public TripleWhaleApiClient(IHttpClientFactory httpClientFactory, TripleWhaleSettings settings)
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings;
    }

    /// <summary>
    /// Validates an API key (defaults to the configured key)
    /// </summary>
    public Task<TripleWhaleApiResult> ValidateApiKeyAsync(string apiKey = null)
        => SendJsonAsync(HttpMethod.Get, TripleWhaleDefaults.ValidateApiKeyEndpoint, null, apiKey);

    public Task<TripleWhaleApiResult> SendOrderAsync(OrderRecordDto order)
        => SendJsonAsync(HttpMethod.Post, TripleWhaleDefaults.OrdersEndpoint, Serialize(order));

    public Task<TripleWhaleApiResult> SendCustomerAsync(CustomerRecordDto customer)
        => SendJsonAsync(HttpMethod.Post, TripleWhaleDefaults.CustomersEndpoint, Serialize(customer));

    public Task<TripleWhaleApiResult> SendProductAsync(ProductRecordDto product)
        => SendJsonAsync(HttpMethod.Post, TripleWhaleDefaults.ProductsEndpoint, Serialize(product));

    public Task<TripleWhaleApiResult> SendEventAsync(OfflineEventDto offlineEvent)
        => SendJsonAsync(HttpMethod.Post, TripleWhaleDefaults.EventsEndpoint, Serialize(offlineEvent));

    /// <summary>
    /// Sends a pre-serialized JSON payload (used by the sync queue to replay stored payloads)
    /// </summary>
    public Task<TripleWhaleApiResult> SendRawAsync(HttpMethod method, string endpoint, string jsonPayload)
        => SendJsonAsync(method, endpoint, jsonPayload);

    private static string Serialize(object payload)
        => payload is null ? null : JsonSerializer.Serialize(payload, SerializerOptions);

    private async Task<TripleWhaleApiResult> SendJsonAsync(HttpMethod method, string endpoint, string json, string apiKey = null)
    {
        var key = string.IsNullOrWhiteSpace(apiKey) ? _settings.ApiKey : apiKey;

        for (var attempt = 1; attempt <= TripleWhaleDefaults.MaxRetryAttempts; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(method, TripleWhaleDefaults.ApiBaseUrl + endpoint);
                request.Headers.Add(TripleWhaleDefaults.ApiKeyHeader, key);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (json is not null)
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var client = _httpClientFactory.CreateClient();
                using var response = await client.SendAsync(request);

                var body = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                    return new TripleWhaleApiResult
                    {
                        Success = true,
                        StatusCode = statusCode,
                        ResponseBody = body,
                        Message = body
                    };

                // transient failures: 429 (rate limit) and 5xx server errors
                var retriable = statusCode == (int)HttpStatusCode.TooManyRequests || statusCode >= 500;

                if (!retriable || attempt == TripleWhaleDefaults.MaxRetryAttempts)
                    return new TripleWhaleApiResult
                    {
                        Success = false,
                        StatusCode = statusCode,
                        ResponseBody = body,
                        Message = ExtractMessage(body) ?? response.ReasonPhrase,
                        ShouldRetry = retriable
                    };

                await Task.Delay(GetRetryDelay(attempt, response));
            }
            catch (Exception exception) when (IsTransient(exception))
            {
                if (attempt == TripleWhaleDefaults.MaxRetryAttempts)
                    return new TripleWhaleApiResult
                    {
                        Success = false,
                        StatusCode = 0,
                        Message = exception.Message,
                        ShouldRetry = true
                    };

                await Task.Delay(GetRetryDelay(attempt, null));
            }
        }

        return new TripleWhaleApiResult
        {
            Success = false,
            StatusCode = 0,
            Message = "Unexpected error",
            ShouldRetry = true
        };
    }

    private static bool IsTransient(Exception exception)
        => exception is HttpRequestException or TaskCanceledException or OperationCanceledException or TimeoutException;

    private static TimeSpan GetRetryDelay(int attempt, HttpResponseMessage response)
    {
        // honor Retry-After header (429) but cap the wait to avoid blocking too long
        if (response?.Headers.RetryAfter is { } retryAfter)
        {
            var serverDelay = retryAfter.Delta ??
                              (retryAfter.Date is { } date ? date - DateTimeOffset.UtcNow : (TimeSpan?)null);

            if (serverDelay is { } delay && delay > TimeSpan.Zero)
            {
                var max = TimeSpan.FromMilliseconds(TripleWhaleDefaults.RetryMaxDelayMilliseconds);
                return delay > max ? max : delay;
            }
        }

        var delayMs = TripleWhaleDefaults.RetryBaseDelayMilliseconds * Math.Pow(2, attempt - 1);
        delayMs = Math.Min(delayMs, TripleWhaleDefaults.RetryMaxDelayMilliseconds);
        return TimeSpan.FromMilliseconds(delayMs);
    }

    private static string ExtractMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("message", out var message) &&
                message.ValueKind == JsonValueKind.String)
                return message.GetString();
        }
        catch (JsonException)
        {
            // ignore; fall back to the raw body
        }

        return body;
    }
}