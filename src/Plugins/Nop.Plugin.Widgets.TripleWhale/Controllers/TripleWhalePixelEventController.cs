using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.TripleWhale.Domain;
using Nop.Plugin.Widgets.TripleWhale.Services;

namespace Nop.Plugin.Widgets.TripleWhale.Controllers;

/// <summary>
/// Public endpoint that logs client-side pixel events to the database
/// </summary>
[Route("Plugins/TripleWhale/PixelEvent")]
public class TripleWhalePixelEventController : Controller
{
    private const int MaxBodyBytes = 4096;
    private const int MaxRequestsPerMinute = 60;

    private static readonly HashSet<string> AllowedEvents = new(StringComparer.OrdinalIgnoreCase)
    {
        "AddToCart", "RemoveFromCart", "AddToWishlist", "ViewContent",
        "InitiateCheckout", "AddressSubmitted", "ShippingSubmitted", "AddPaymentInfo",
        "Purchase", "Search", "Contact", "SignUp", "Lead", "custom"
    };

    private static readonly ConcurrentDictionary<string, RateLimitEntry> RateLimit = new();

    private readonly TripleWhaleSettings _settings;
    private readonly TripleWhaleLogService _logService;

    public TripleWhalePixelEventController(TripleWhaleSettings settings, TripleWhaleLogService logService)
    {
        _settings = settings;
        _logService = logService;
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Index()
    {
        if (!_settings.EnablePixel)
            return new EmptyResult();

        if (!IsAllowed())
            return StatusCode(StatusCodes.Status429TooManyRequests);

        if (Request.ContentLength.HasValue && Request.ContentLength.Value > MaxBodyBytes)
            return new EmptyResult();

        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        if (body.Length > MaxBodyBytes)
            return new EmptyResult();

        string eventName = null;
        string dataJson = null;

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("eventName", out var nameEl))
                eventName = nameEl.GetString();
            if (doc.RootElement.TryGetProperty("data", out var dataEl))
                dataJson = dataEl.ToString();
        }
        catch (JsonException)
        {
            return new EmptyResult();
        }

        if (string.IsNullOrWhiteSpace(eventName) || !AllowedEvents.Contains(eventName))
            return new EmptyResult();

        await _logService.LogAsync(TripleWhaleLogType.Pixel, eventName, dataJson ?? "{}");

        return new EmptyResult();
    }

    private bool IsAllowed()
    {
        var key = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;

        if (RateLimit.Count > 10_000)
        {
            foreach (var stale in RateLimit.Where(kv => now - kv.Value.WindowStart > TimeSpan.FromMinutes(2)).Select(kv => kv.Key).ToList())
                RateLimit.TryRemove(stale, out _);
        }

        var entry = RateLimit.AddOrUpdate(key,
            _ => new RateLimitEntry(now),
            (_, existing) =>
            {
                if (now - existing.WindowStart > TimeSpan.FromMinutes(1))
                    return new RateLimitEntry(now);

                Interlocked.Increment(ref existing.Count);
                return existing;
            });

        return entry.Count <= MaxRequestsPerMinute;
    }

    private sealed class RateLimitEntry
    {
        public RateLimitEntry(DateTime windowStart)
        {
            WindowStart = windowStart;
            Count = 1;
        }

        public DateTime WindowStart { get; set; }

        public int Count;
    }
}
