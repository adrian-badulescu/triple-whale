using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Widgets.TripleWhale.Services;

/// <summary>
/// Scheduled task that deletes old Triple Whale log entries
/// </summary>
public class DeleteTripleWhaleLogsTask : IScheduleTask
{
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly TripleWhaleLogService _logService;

    public DeleteTripleWhaleLogsTask(
        IStoreContext storeContext,
        ISettingService settingService,
        TripleWhaleLogService logService)
    {
        _storeContext = storeContext;
        _settingService = settingService;
        _logService = logService;
    }

    public async Task ExecuteAsync()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<TripleWhaleSettings>(storeScope);

        var days = settings.NumberOfDaysToKeepLogs > 0
            ? settings.NumberOfDaysToKeepLogs
            : TripleWhaleDefaults.DefaultNumberOfDaysToKeepLogs;

        await _logService.DeleteOldLogsAsync(days);
    }
}
