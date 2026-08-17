namespace Nop.Plugin.Widgets.TripleWhale;

/// <summary>
/// Represents plugin constants
/// </summary>
public static class TripleWhaleDefaults
{
    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    public static string SystemName => "Widgets.TripleWhale";

    /// <summary>
    /// Gets the configuration route name
    /// </summary>
    public static string ConfigurationRouteName => "Plugin.Widgets.TripleWhale.Configure";

    /// <summary>
    /// Gets the Triple Whale API base URL
    /// </summary>
    public static string ApiBaseUrl => "https://api.triplewhale.com/api/v2";

    /// <summary>
    /// Gets the validate API key endpoint
    /// </summary>
    public static string ValidateApiKeyEndpoint => "/users/api-keys/me";

    /// <summary>
    /// Gets the data-in orders endpoint
    /// </summary>
    public static string OrdersEndpoint => "/data-in/orders";

    /// <summary>
    /// Gets the data-in bulk orders endpoint
    /// </summary>
    public static string BulkOrdersEndpoint => "/data-in/orders/bulk";

    /// <summary>
    /// Gets the data-in customers endpoint
    /// </summary>
    public static string CustomersEndpoint => "/data-in/customers";

    /// <summary>
    /// Gets the data-in products endpoint
    /// </summary>
    public static string ProductsEndpoint => "/data-in/products";

    /// <summary>
    /// Gets the data-in offline events endpoint
    /// </summary>
    public static string EventsEndpoint => "/data-in/event";

    /// <summary>
    /// Gets the HTTP header used to authenticate API requests
    /// </summary>
    public static string ApiKeyHeader => "x-api-key";

    /// <summary>
    /// Gets the maximum number of retry attempts for transient API failures (initial call + retries)
    /// </summary>
    public static int MaxRetryAttempts => 4;

    /// <summary>
    /// Gets the initial retry backoff delay in milliseconds
    /// </summary>
    public static int RetryBaseDelayMilliseconds => 2000;

    /// <summary>
    /// Gets the maximum retry backoff delay in milliseconds
    /// </summary>
    public static int RetryMaxDelayMilliseconds => 32000;

    /// <summary>
    /// Gets the maximum number of persisted retry attempts for a sync queue item
    /// </summary>
    public static int MaxQueueRetryAttempts => 5;

    /// <summary>
    /// Gets the base delay (in minutes) between sync queue retries
    /// </summary>
    public static int QueueRetryBaseMinutes => 5;

    /// <summary>
    /// Gets the maximum delay (in minutes) between sync queue retries
    /// </summary>
    public static int QueueRetryMaxMinutes => 360;

    /// <summary>
    /// Gets the scheduled background sync task type
    /// </summary>
    public static string BackgroundSyncTaskType => "Nop.Plugin.Widgets.TripleWhale.Services.TripleWhaleBackgroundSyncTask";

    /// <summary>
    /// Gets the scheduled background sync task name
    /// </summary>
    public static string BackgroundSyncTaskName => "Triple Whale sync (Triple Whale plugin)";

    /// <summary>
    /// Gets the background sync interval in seconds (5 minutes)
    /// </summary>
    public static int BackgroundSyncIntervalSeconds => 300;

    /// <summary>
    /// Gets the session key used to store pending Triple Whale pixel events
    /// </summary>
    public static string TrackedEventsSessionValue => "TripleWhaleTrackedEvents";
}
