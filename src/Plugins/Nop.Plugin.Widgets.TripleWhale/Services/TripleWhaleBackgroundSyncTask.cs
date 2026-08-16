using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Widgets.TripleWhale.Services;

/// <summary>
/// Background scheduled task that drains the Triple Whale sync queue
/// </summary>
public class TripleWhaleBackgroundSyncTask : IScheduleTask
{
    private readonly TripleWhaleSyncService _syncService;

    public TripleWhaleBackgroundSyncTask(TripleWhaleSyncService syncService)
    {
        _syncService = syncService;
    }

    /// <summary>
    /// Executes the task
    /// </summary>
    public async Task ExecuteAsync()
    {
        await _syncService.ProcessQueueAsync();
    }
}
