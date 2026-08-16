using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Widgets.TripleWhale.Domain;

namespace Nop.Plugin.Widgets.TripleWhale.Data;

/// <summary>
/// Triple Whale sync queue builder
/// </summary>
public class TripleWhaleSyncQueueBuilder : NopEntityBuilder<TripleWhaleSyncQueueRecord>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(TripleWhaleSyncQueueRecord.EntityId)).AsString(100).Nullable()
            .WithColumn(nameof(TripleWhaleSyncQueueRecord.EventType)).AsString(100).Nullable()
            .WithColumn(nameof(TripleWhaleSyncQueueRecord.Payload)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(TripleWhaleSyncQueueRecord.LastError)).AsString(1000).Nullable();
    }
}
