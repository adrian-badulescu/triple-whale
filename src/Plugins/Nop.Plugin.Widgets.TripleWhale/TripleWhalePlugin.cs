using Nop.Core.Domain.Cms;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Plugin.Widgets.TripleWhale.Components;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
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
    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly ISettingService _settingService;
    private readonly WidgetSettings _widgetSettings;

    public TripleWhalePlugin(ILocalizationService localizationService,
        INopUrlHelper nopUrlHelper,
        IScheduleTaskService scheduleTaskService,
        ISettingService settingService,
        WidgetSettings widgetSettings)
    {
        _localizationService = localizationService;
        _nopUrlHelper = nopUrlHelper;
        _scheduleTaskService = scheduleTaskService;
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
            PublicWidgetZones.HeadHtmlTag,
            PublicWidgetZones.OpcContentBefore
        });
    }

    /// <summary>
    /// Gets a type of a view component for displaying the widget
    /// </summary>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        ArgumentNullException.ThrowIfNull(widgetZone);

        if (widgetZone.Equals(PublicWidgetZones.HeadHtmlTag) || widgetZone.Equals(PublicWidgetZones.OpcContentBefore))
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
            ["Plugins.Widgets.TripleWhale.Fields.ShopUrl.Hint"] = "Leave empty to auto-detect from the store URL. Must match the Shop URL in Triple Whale (Settings → Store) and the TripleName in your pixel snippet.",
            ["Plugins.Widgets.TripleWhale.Fields.PixelSnippet"] = "Pixel snippet",
            ["Plugins.Widgets.TripleWhale.Fields.PixelSnippet.Hint"] = "Paste the full base tracking snippet from the Triple Whale dashboard (Headless Installation).",
            ["Plugins.Widgets.TripleWhale.Fields.EnablePixel"] = "Enable pixel",
            ["Plugins.Widgets.TripleWhale.Fields.EnableServerSideEvents"] = "Enable server-side events",
            ["Plugins.Widgets.TripleWhale.Fields.EnableOrderSync"] = "Enable order sync",
            ["Plugins.Widgets.TripleWhale.Fields.EnableCustomerSync"] = "Enable customer sync",
            ["Plugins.Widgets.TripleWhale.Fields.EnableProductSync"] = "Enable product sync",

            ["Plugins.Widgets.TripleWhale.Instructions"] = "Enter your Triple Whale API key and shop URL below. The pixel snippet is optional and only required for client-side tracking.",
            ["Plugins.Widgets.TripleWhale.Fields.ApiKey.Required"] = "API key is required",
            ["Plugins.Widgets.TripleWhale.Fields.ShopUrl.Required"] = "Shop URL is required",
            ["Plugins.Widgets.TripleWhale.Button.TestConnection"] = "Test connection",
            ["Plugins.Widgets.TripleWhale.Button.SyncAll"] = "Sync all",
            ["Plugins.Widgets.TripleWhale.Button.DownloadLogs"] = "Download logs",
            ["Plugins.Widgets.TripleWhale.TestConnection.Success"] = "Connection to Triple Whale was successful.",
            ["Plugins.Widgets.TripleWhale.TestConnection.Failed"] = "Connection to Triple Whale failed: {0}",
            ["Plugins.Widgets.TripleWhale.SyncAll.Success"] = "{0} item(s) were queued for sync to Triple Whale."
        });

        // schedule the background sync queue processor
        await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
        {
            Type = TripleWhaleDefaults.BackgroundSyncTaskType,
            Name = TripleWhaleDefaults.BackgroundSyncTaskName,
            Seconds = TripleWhaleDefaults.BackgroundSyncIntervalSeconds,
            Enabled = true
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

        var syncTask = await _scheduleTaskService.GetTaskByTypeAsync(TripleWhaleDefaults.BackgroundSyncTaskType);
        if (syncTask is not null)
            await _scheduleTaskService.DeleteTaskAsync(syncTask);

        await base.UninstallAsync();
    }

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => false;
}
