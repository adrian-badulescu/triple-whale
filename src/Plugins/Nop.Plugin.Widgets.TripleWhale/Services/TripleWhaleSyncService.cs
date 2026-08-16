using System.Net.Http;
using System.Text.Json;
using Nop.Data;
using Nop.Plugin.Widgets.TripleWhale.Domain;

namespace Nop.Plugin.Widgets.TripleWhale.Services;

/// <summary>
/// Manages the Triple Whale sync queue (retry queue)
/// </summary>
public class TripleWhaleSyncService
{
    private readonly IRepository<TripleWhaleSyncQueueRecord> _queueRepository;
    private readonly TripleWhaleApiClient _apiClient;
    private readonly TripleWhaleLogService _logService;
    private readonly TripleWhaleSettings _settings;

    public TripleWhaleSyncService(
        IRepository<TripleWhaleSyncQueueRecord> queueRepository,
        TripleWhaleApiClient apiClient,
        TripleWhaleLogService logService,
        TripleWhaleSettings settings)
    {
        _queueRepository = queueRepository;
        _apiClient = apiClient;
        _logService = logService;
        _settings = settings;
    }

    /// <summary>
    /// Adds an item to the sync queue for asynchronous delivery to Triple Whale
    /// </summary>
    public async Task EnqueueAsync(TripleWhaleEntityType entityType, string entityId, string eventType, object payload)
    {
        var json = payload is null ? null : JsonSerializer.Serialize(payload, TripleWhaleApiClient.SerializerOptions);

        await _queueRepository.InsertAsync(new TripleWhaleSyncQueueRecord
        {
            EntityTypeId = (int)entityType,
            EntityId = entityId,
            EventType = eventType,
            Payload = json,
            StatusId = (int)TripleWhaleQueueStatus.Pending,
            RetryCount = 0,
            CreatedDateUtc = DateTime.UtcNow,
            UpdatedDateUtc = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Processes up to <paramref name="batchSize"/> due queue items and returns the number processed
    /// </summary>
    public async Task<int> ProcessQueueAsync(int batchSize = 50)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            return 0;

        var now = DateTime.UtcNow;
        var staleProcessingThreshold = now.AddMinutes(-10);

        var dueItems = _queueRepository.Table
            .Where(item => item.RetryCount < TripleWhaleDefaults.MaxQueueRetryAttempts &&
                           (item.StatusId == (int)TripleWhaleQueueStatus.Pending ||
                            (item.StatusId == (int)TripleWhaleQueueStatus.Failed &&
                             (item.NextRetryUtc == null || item.NextRetryUtc <= now)) ||
                            (item.StatusId == (int)TripleWhaleQueueStatus.Processing &&
                             item.UpdatedDateUtc < staleProcessingThreshold)))
            .OrderBy(item => item.CreatedDateUtc)
            .Take(batchSize)
            .ToList();

        var processed = 0;
        foreach (var item in dueItems)
        {
            await ProcessItemAsync(item);
            processed++;
        }

        return processed;
    }

    private async Task ProcessItemAsync(TripleWhaleSyncQueueRecord item)
    {
        item.Status = TripleWhaleQueueStatus.Processing;
        item.UpdatedDateUtc = DateTime.UtcNow;
        await _queueRepository.UpdateAsync(item);

        var endpoint = GetEndpoint(item.EntityType);
        var result = string.IsNullOrEmpty(endpoint)
            ? new TripleWhaleApiResult { Success = false, Message = "Unknown entity type" }
            : await _apiClient.SendRawAsync(HttpMethod.Post, endpoint, item.Payload);

        if (result.Success)
        {
            item.Status = TripleWhaleQueueStatus.Sent;
            item.LastError = null;
            item.NextRetryUtc = null;
            await _logService.LogAsync(TripleWhaleLogType.Info, endpoint, item.Payload, result.ResponseBody);
        }
        else
        {
            item.RetryCount++;
            item.LastError = result.Message;

            if (result.ShouldRetry && item.RetryCount < TripleWhaleDefaults.MaxQueueRetryAttempts)
            {
                item.Status = TripleWhaleQueueStatus.Failed;
                item.NextRetryUtc = CalculateNextRetryUtc(item.RetryCount);
            }
            else
            {
                // permanent failure or retry budget exhausted
                item.Status = TripleWhaleQueueStatus.Failed;
                item.NextRetryUtc = null;
            }

            await _logService.LogAsync(TripleWhaleLogType.Error, endpoint, item.Payload, result.ResponseBody, result.Message);
        }

        item.UpdatedDateUtc = DateTime.UtcNow;
        await _queueRepository.UpdateAsync(item);
    }

    private static string GetEndpoint(TripleWhaleEntityType entityType) => entityType switch
    {
        TripleWhaleEntityType.Order => TripleWhaleDefaults.OrdersEndpoint,
        TripleWhaleEntityType.Customer => TripleWhaleDefaults.CustomersEndpoint,
        TripleWhaleEntityType.Product => TripleWhaleDefaults.ProductsEndpoint,
        TripleWhaleEntityType.Event => TripleWhaleDefaults.EventsEndpoint,
        _ => null
    };

    private static DateTime CalculateNextRetryUtc(int retryCount)
    {
        var minutes = TripleWhaleDefaults.QueueRetryBaseMinutes * Math.Pow(2, retryCount - 1);
        minutes = Math.Min(minutes, TripleWhaleDefaults.QueueRetryMaxMinutes);
        return DateTime.UtcNow.AddMinutes(minutes);
    }
}