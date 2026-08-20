namespace Nop.Plugin.Widgets.TripleWhale;

/// <summary>
/// Provides helpers for resolving the Triple Whale shop domain from the store URL
/// </summary>
public static class TripleWhaleStoreHelper
{
    /// <summary>
    /// Extracts the host (domain) from a store URL
    /// </summary>
    public static string GetHost(string storeUrl)
    {
        if (string.IsNullOrWhiteSpace(storeUrl))
            return null;

        return Uri.TryCreate(storeUrl, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host)
            ? uri.Host
            : null;
    }

    /// <summary>
    /// Normalizes a shop value to the bare domain Triple Whale expects (no scheme, no trailing slash)
    /// </summary>
    public static string NormalizeShop(string shopUrl)
    {
        if (string.IsNullOrWhiteSpace(shopUrl))
            return null;

        var value = shopUrl.Trim();

        // if a full URL was pasted (e.g. https://demo.example.com/), keep only the host
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
            value = uri.Host;

        return value.TrimEnd('/').Trim();
    }
}
