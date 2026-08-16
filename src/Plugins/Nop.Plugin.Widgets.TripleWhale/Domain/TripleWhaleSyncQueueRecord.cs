using Nop.Core;

namespace Nop.Plugin.Widgets.TripleWhale.Domain;

/// <summary>
/// Represents an item in the Triple Whale sync queue (retry queue)
/// </summary>
public class TripleWhaleSyncQueueRecord : BaseEntity
{
    /// <summary>
    /// Gets or sets the entity type identifier
    /// </summary>
    public int EntityTypeId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the entity (order id, customer id, product id)
    /// </summary>
    public string EntityId { get; set; }

    /// <summary>
    /// Gets or sets the event type (e.g. OrderPlaced, purchase, custom)
    /// </summary>
    public string EventType { get; set; }

    /// <summary>
    /// Gets or sets the JSON payload to send to Triple Whale
    /// </summary>
    public string Payload { get; set; }

    /// <summary>
    /// Gets or sets the queue status identifier
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the number of retry attempts
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the next retry (UTC)
    /// </summary>
    public DateTime? NextRetryUtc { get; set; }

    /// <summary>
    /// Gets or sets the last error message
    /// </summary>
    public string LastError { get; set; }

    /// <summary>
    /// Gets or sets the date and time the record was created (UTC)
    /// </summary>
    public DateTime CreatedDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time the record was last updated (UTC)
    /// </summary>
    public DateTime UpdatedDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the entity type
    /// </summary>
    public TripleWhaleEntityType EntityType
    {
        get => (TripleWhaleEntityType)EntityTypeId;
        set => EntityTypeId = (int)value;
    }

    /// <summary>
    /// Gets or sets the queue status
    /// </summary>
    public TripleWhaleQueueStatus Status
    {
        get => (TripleWhaleQueueStatus)StatusId;
        set => StatusId = (int)value;
    }
}
