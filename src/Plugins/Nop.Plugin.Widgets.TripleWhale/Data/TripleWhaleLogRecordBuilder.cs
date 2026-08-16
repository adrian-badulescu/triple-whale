using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Widgets.TripleWhale.Domain;

namespace Nop.Plugin.Widgets.TripleWhale.Data;

/// <summary>
/// Triple Whale log record builder
/// </summary>
public class TripleWhaleLogRecordBuilder : NopEntityBuilder<TripleWhaleLogRecord>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(TripleWhaleLogRecord.Endpoint)).AsString(200).Nullable()
            .WithColumn(nameof(TripleWhaleLogRecord.RequestMessage)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(TripleWhaleLogRecord.ResponseMessage)).AsString(int.MaxValue).Nullable()
            .WithColumn(nameof(TripleWhaleLogRecord.ErrorMessage)).AsString(1000).Nullable();
    }
}
