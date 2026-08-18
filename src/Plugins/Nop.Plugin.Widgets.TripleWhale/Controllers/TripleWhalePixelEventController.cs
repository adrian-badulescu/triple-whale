using System.Text.Json;
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

        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

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

        if (string.IsNullOrWhiteSpace(eventName))
            return new EmptyResult();

        await _logService.LogAsync(TripleWhaleLogType.Pixel, eventName, dataJson ?? "{}");

        return new EmptyResult();
    }
}
