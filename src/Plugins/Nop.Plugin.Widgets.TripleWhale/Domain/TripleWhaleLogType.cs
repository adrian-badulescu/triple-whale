namespace Nop.Plugin.Widgets.TripleWhale.Domain;

/// <summary>
/// Represents a log severity level
/// </summary>
public enum TripleWhaleLogType
{
    /// <summary>
    /// Informational message
    /// </summary>
    Info = 1,

    /// <summary>
    /// Warning (non-fatal issue)
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error
    /// </summary>
    Error = 3,

    /// <summary>
    /// Pixel event (client-side tracking event)
    /// </summary>
    Pixel = 4
}
