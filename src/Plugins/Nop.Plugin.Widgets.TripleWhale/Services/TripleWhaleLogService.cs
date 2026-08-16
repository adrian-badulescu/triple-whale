using Nop.Data;
using Nop.Plugin.Widgets.TripleWhale.Domain;

namespace Nop.Plugin.Widgets.TripleWhale.Services;

/// <summary>
/// Writes and manages Triple Whale API log records
/// </summary>
public class TripleWhaleLogService
{
    private readonly IRepository<TripleWhaleLogRecord> _logRepository;

    public TripleWhaleLogService(IRepository<TripleWhaleLogRecord> logRepository)
    {
        _logRepository = logRepository;
    }

    /// <summary>
    /// Writes a log entry
    /// </summary>
    public async Task LogAsync(TripleWhaleLogType logType, string endpoint,
        string requestMessage = null, string responseMessage = null, string errorMessage = null)
    {
        await _logRepository.InsertAsync(new TripleWhaleLogRecord
        {
            CorrelationId = Guid.NewGuid(),
            LogTypeId = (int)logType,
            Endpoint = endpoint,
            RequestMessage = requestMessage,
            ResponseMessage = responseMessage,
            ErrorMessage = errorMessage,
            CreatedDateUtc = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Deletes log entries older than the specified number of days
    /// </summary>
    public async Task DeleteOldLogsAsync(int numberOfDaysToKeep)
    {
        if (numberOfDaysToKeep <= 0)
            return;

        var threshold = DateTime.UtcNow.AddDays(-numberOfDaysToKeep);
        var oldLogs = _logRepository.Table
            .Where(log => log.CreatedDateUtc < threshold)
            .ToList();

        if (oldLogs.Any())
            await _logRepository.DeleteAsync(oldLogs);
    }
}