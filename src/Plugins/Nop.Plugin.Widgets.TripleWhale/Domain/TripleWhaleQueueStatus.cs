namespace Nop.Plugin.Widgets.TripleWhale.Domain;

/// <summary>
/// Represents the sync queue item status
/// </summary>
public enum TripleWhaleQueueStatus
{
    /// <summary>
    /// Waiting to be processed
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Currently being processed
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Successfully sent
    /// </summary>
    Sent = 3,

    /// <summary>
    /// Failed (will be retried)
    /// </summary>
    Failed = 4
}
