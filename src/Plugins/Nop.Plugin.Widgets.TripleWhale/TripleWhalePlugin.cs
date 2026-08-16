using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.TripleWhale.Components;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Widgets.TripleWhale;

/// <summary>
/// Triple Whale plugin
/// </summary>
public class TripleWhalePlugin : BasePlugin, IWidgetPlugin
{
    private readonly ILocalizationService _localizationService;
    private readonly INopUrlHelper _nopUrlHelper;
    private readonly ISettingService _settingService;
    private readonly WidgetSettings _widgetSettings;

    public TripleWhalePlugin(ILocalizationService localizationService,
        INopUrlHelper nopUrlHelper,
        ISettingService settingService,
        WidgetSettings widgetSettings)
    {
        _localizationService = localizationService;
        _nopUrlHelper = nopUrlHelper;
        _settingService = settingService;
        _widgetSettings = widgetSettings;
    }

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string>
        {
            PublicWidgetZones.HeadHtmlTag
        });
    }

    /// <summary>
    /// Gets a type of a view component for displaying the widget
    /// </summary>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        ArgumentNullException.ThrowIfNull(widgetZone);

        if (widgetZone.Equals(PublicWidgetZones.HeadHtmlTag))
            return typeof(TripleWhalePixelViewComponent);

        return null;
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _nopUrlHelper.RouteUrl(TripleWhaleDefaults.ConfigurationRouteName);
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new TripleWhaleSettings
        {
            EnablePixel = true,
            EnableServerSideEvents = true,
            EnableOrderSync = true,
            EnableCustomerSync = true,
            EnableProductSync = true
        });

        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(TripleWhaleDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(TripleWhaleDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Widgets.TripleWhale.Fields.ApiKey"] = "API key",
            ["Plugins.Widgets.TripleWhale.Fields.ApiKey.Hint"] = "Your Triple Whale API key.",
            ["Plugins.Widgets.TripleWhale.Fields.ShopUrl"] = "Shop URL",
            ["Plugins.Widgets.TripleWhale.Fields.ShopUrl.Hint"] = "Must match the Shop URL in Triple Whale (Settings → Store).",
            ["Plugins.Widgets.TripleWhale.Fields.PixelSnippet"] = "Pixel snippet",
            ["Plugins.Widgets.TripleWhale.Fields.PixelSnippet.Hint"] = "Paste the full base tracking snippet from the Triple Whale dashboard (Headless Installation).",
            ["Plugins.Widgets.TripleWhale.Fields.EnablePixel"] = "Enable pixel",
            ["Plugins.Widgets.TripleWhale.Fields.EnableServerSideEvents"] = "Enable server-side events",
            ["Plugins.Widgets.TripleWhale.Fields.EnableOrderSync"] = "Enable order sync",
            ["Plugins.Widgets.TripleWhale.Fields.EnableCustomerSync"] = "Enable customer sync",
            ["Plugins.Widgets.TripleWhale.Fields.EnableProductSync"] = "Enable product sync"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    public override async Task UninstallAsync()
    {
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(TripleWhaleDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(TripleWhaleDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _settingService.DeleteSettingAsync<TripleWhaleSettings>();
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.TripleWhale");

        await base.UninstallAsync();
    }

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => false;
}
