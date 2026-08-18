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
}
