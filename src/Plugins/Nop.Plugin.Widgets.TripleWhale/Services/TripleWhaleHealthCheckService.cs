namespace Nop.Plugin.Widgets.TripleWhale.Services;

/// <summary>
/// Validates the Triple Whale connection / API key
/// </summary>
public class TripleWhaleHealthCheckService
{
    private readonly TripleWhaleApiClient _apiClient;
    private readonly TripleWhaleSettings _settings;

    public TripleWhaleHealthCheckService(TripleWhaleApiClient apiClient, TripleWhaleSettings settings)
    {
        _apiClient = apiClient;
        _settings = settings;
    }

    /// <summary>
    /// Validates the configured API key (or an explicit key) against the Triple Whale API
    /// </summary>
    public async Task<TripleWhaleApiResult> CheckAsync(string apiKey = null)
    {
        var key = string.IsNullOrWhiteSpace(apiKey) ? _settings.ApiKey : apiKey;

        if (string.IsNullOrWhiteSpace(key))
            return new TripleWhaleApiResult { Success = false, Message = "API key is not configured" };

        return await _apiClient.ValidateApiKeyAsync(key);
    }
}