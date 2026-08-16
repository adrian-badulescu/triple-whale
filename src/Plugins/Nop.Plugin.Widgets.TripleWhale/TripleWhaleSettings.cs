using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.TripleWhale;

/// <summary>
/// Represents Triple Whale settings
/// </summary>
public class TripleWhaleSettings : ISettings
{
    /// <summary>
    /// Gets or sets the Triple Whale API key (encrypted at rest)
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the shop URL (must match Settings → Store in Triple Whale)
    /// </summary>
    public string ShopUrl { get; set; }

    /// <summary>
    /// Gets or sets the full pixel tracking snippet
    /// </summary>
    public string PixelSnippet { get; set; }

    /// <summary>
    /// Gets or sets whether the pixel is enabled
    /// </summary>
    public bool EnablePixel { get; set; }

    /// <summary>
    /// Gets or sets whether server-side events are enabled
    /// </summary>
    public bool EnableServerSideEvents { get; set; }

    /// <summary>
    /// Gets or sets whether order sync is enabled
    /// </summary>
    public bool EnableOrderSync { get; set; }

    /// <summary>
    /// Gets or sets whether customer sync is enabled
    /// </summary>
    public bool EnableCustomerSync { get; set; }

    /// <summary>
    /// Gets or sets whether product sync is enabled
    /// </summary>
    public bool EnableProductSync { get; set; }
}
