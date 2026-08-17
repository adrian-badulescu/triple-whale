using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.TripleWhale.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.TripleWhale.Components;

/// <summary>
/// View component that injects the Triple Whale pixel snippet and tracked events into the head
/// </summary>
public class TripleWhalePixelViewComponent : NopViewComponent
{
    private readonly TripleWhalePixelService _tripleWhalePixelService;

    public TripleWhalePixelViewComponent(TripleWhalePixelService tripleWhalePixelService)
    {
        _tripleWhalePixelService = tripleWhalePixelService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var script = await _tripleWhalePixelService.PrepareScriptAsync();

        return View("~/Plugins/Widgets.TripleWhale/Views/PublicInfo.cshtml", script);
    }
}
