using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.TripleWhale.Components;

/// <summary>
/// View component that injects the Triple Whale pixel snippet into the head
/// </summary>
public class TripleWhalePixelViewComponent : NopViewComponent
{
    private readonly TripleWhaleSettings _tripleWhaleSettings;

    public TripleWhalePixelViewComponent(TripleWhaleSettings tripleWhaleSettings)
    {
        _tripleWhaleSettings = tripleWhaleSettings;
    }

    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        var snippet = string.Empty;

        if (_tripleWhaleSettings.EnablePixel && !string.IsNullOrWhiteSpace(_tripleWhaleSettings.PixelSnippet))
            snippet = _tripleWhaleSettings.PixelSnippet;

        return View("~/Plugins/Widgets.TripleWhale/Views/PublicInfo.cshtml", snippet);
    }
}
