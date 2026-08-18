using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Http.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Widgets.TripleWhale.Services;

/// <summary>
/// Manages the Triple Whale pixel: injects the base snippet and queues tracked
/// events (Contact, AddToCart, custom) so they are rendered on the next page.
/// </summary>
public class TripleWhalePixelService
{
    private readonly TripleWhaleSettings _settings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProductService _productService;
    private readonly IStoreContext _storeContext;

    public TripleWhalePixelService(TripleWhaleSettings settings,
        IHttpContextAccessor httpContextAccessor,
        IProductService productService,
        IStoreContext storeContext)
    {
        _settings = settings;
        _httpContextAccessor = httpContextAccessor;
        _productService = productService;
        _storeContext = storeContext;
    }

    /// <summary>
    /// Prepares the pixel script: the base snippet plus any pending tracked events
    /// </summary>
    public async Task<string> PrepareScriptAsync()
    {
        if (!_settings.EnablePixel)
            return string.Empty;

        var snippet = _settings.PixelSnippet;
        if (string.IsNullOrWhiteSpace(snippet))
            return string.Empty;

        snippet = await InjectStoreDomainAsync(snippet);

        return snippet + await PrepareTrackedEventsScriptAsync();
    }

    /// <summary>
    /// Queues an AddToCart event for the given shopping cart item
    /// </summary>
    public async Task SendAddToCartEventAsync(ShoppingCartItem item)
    {
        if (item is null || item.ProductId == 0 || item.ShoppingCartType != ShoppingCartType.ShoppingCart)
            return;

        var product = await _productService.GetProductByIdAsync(item.ProductId);

        var properties = new Dictionary<string, object>
        {
            ["item"] = item.ProductId.ToString(),
            ["q"] = item.Quantity
        };

        if (!string.IsNullOrWhiteSpace(product?.Sku))
            properties["v"] = product.Sku;

        await QueueScriptAsync($"TriplePixel('AddToCart', {JsonSerializer.Serialize(properties)});");
    }

    /// <summary>
    /// Queues a Contact event with the captured email and/or phone
    /// </summary>
    public async Task SendContactEventAsync(string email, string phone = null)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
            return;

        var properties = new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(email))
            properties["email"] = email;
        if (!string.IsNullOrWhiteSpace(phone))
            properties["phone"] = phone;

        await QueueScriptAsync($"TriplePixel('Contact', {JsonSerializer.Serialize(properties)});");
    }

    /// <summary>
    /// Queues a custom event: TriplePixel('custom', eventName, properties)
    /// </summary>
    public async Task SendCustomEventAsync(string eventName, object properties)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            return;

        await QueueScriptAsync($"TriplePixel('custom', '{EscapeJs(eventName)}', {JsonSerializer.Serialize(properties ?? new { })});");
    }

    private async Task QueueScriptAsync(string script)
    {
        if (!_settings.EnablePixel)
            return;

        var context = _httpContextAccessor.HttpContext;
        if (context?.Session is null)
            return;

        var scripts = await context.Session.GetAsync<IList<string>>(TripleWhaleDefaults.TrackedEventsSessionValue)
                      ?? new List<string>();

        scripts.Add(script);

        await context.Session.SetAsync(TripleWhaleDefaults.TrackedEventsSessionValue, scripts);
    }

    private async Task<string> PrepareTrackedEventsScriptAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.Session is null)
            return string.Empty;

        var scripts = await context.Session.GetAsync<IList<string>>(TripleWhaleDefaults.TrackedEventsSessionValue);
        if (scripts is null || !scripts.Any())
            return string.Empty;

        // render the events only once
        context.Session.Remove(TripleWhaleDefaults.TrackedEventsSessionValue);

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("<!-- Triple Whale tracked events -->");
        sb.AppendLine("<script>");
        foreach (var script in scripts)
            sb.AppendLine(script);
        sb.AppendLine("</script>");

        return sb.ToString();
    }

    /// <summary>
    /// Rewrites the snippet so its TripleName and first?host match the store domain
    /// </summary>
    private async Task<string> InjectStoreDomainAsync(string snippet)
    {
        var domain = await GetShopDomainAsync();
        if (string.IsNullOrWhiteSpace(domain))
            return snippet;

        snippet = Regex.Replace(snippet, @"TripleName:\s*""[^""]*""", $"TripleName:\"{domain}\"");
        snippet = Regex.Replace(snippet, @"first\?host=[^&""']+", $"first?host={domain}");

        return snippet;
    }

    /// <summary>
    /// Gets the Triple Whale shop domain: the configured ShopUrl or the current store host
    /// </summary>
    private async Task<string> GetShopDomainAsync()
    {
        if (!string.IsNullOrWhiteSpace(_settings.ShopUrl))
            return _settings.ShopUrl;

        var store = await _storeContext.GetCurrentStoreAsync();
        return TripleWhaleStoreHelper.GetHost(store?.Url);
    }

    private static string EscapeJs(string value)
        => string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\\", "\\\\").Replace("'", "\\'");
}
