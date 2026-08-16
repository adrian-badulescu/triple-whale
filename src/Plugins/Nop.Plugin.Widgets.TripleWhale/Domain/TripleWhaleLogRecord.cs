using Nop.Core;

namespace Nop.Plugin.Widgets.TripleWhale.Domain;

/// <summary>
/// Represents a Triple Whale API call log entry
/// </summary>
public class TripleWhaleLogRecord : BaseEntity
{
    /// <summary>
    /// Gets or sets the correlation identifier (links request/response pairs and webhook events)
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the log type identifier
    /// </summary>
    public int LogTypeId { get; set; }

    /// <summary>
    /// Gets or sets the API endpoint that was called
    /// </summary>
    public string Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the request payload (JSON)
    /// </summary>
    public string RequestMessage { get; set; }

    /// <summary>
    /// Gets or sets the response body
    /// </summary>
    public string ResponseMessage { get; set; }

    /// <summary>
    /// Gets or sets the error message (if any)
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the date and time the record was created (UTC)
    /// </summary>
    public DateTime CreatedDateUtc { get; set; }

    /// <summary>
    /// Gets or sets the log type
    /// </summary>
    public TripleWhaleLogType LogType
    {
        get => (TripleWhaleLogType)LogTypeId;
        set => LogTypeId = (int)value;
    }
}
