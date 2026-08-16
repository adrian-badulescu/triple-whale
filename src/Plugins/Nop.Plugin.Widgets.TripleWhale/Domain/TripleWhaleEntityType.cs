namespace Nop.Plugin.Widgets.TripleWhale.Domain;

/// <summary>
/// Represents the type of entity a sync queue item refers to
/// </summary>
public enum TripleWhaleEntityType
{
    /// <summary>
    /// Order
    /// </summary>
    Order = 1,

    /// <summary>
    /// Customer
    /// </summary>
    Customer = 2,

    /// <summary>
    /// Product
    /// </summary>
    Product = 3,

    /// <summary>
    /// Offline event
    /// </summary>
    Event = 4
}
