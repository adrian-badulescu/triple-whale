namespace Nop.Plugin.Widgets.TripleWhale.Services;

/// <summary>
/// Represents the result of a Triple Whale API call
/// </summary>
public class TripleWhaleApiResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the call succeeded (HTTP 2xx)
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code (0 when no response was received)
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the server message or error message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the raw response body
    /// </summary>
    public string ResponseBody { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the call should be retried later
    /// </summary>
    public bool ShouldRetry { get; set; }
}