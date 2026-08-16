using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.TripleWhale.Domain;

namespace Nop.Plugin.Widgets.TripleWhale.Data;

/// <summary>
/// Triple Whale schema migration
/// </summary>
[NopMigration("2026/08/16 12:00:00:0000001", "Widgets.TripleWhale base schema", MigrationProcessType.Installation)]
public class TripleWhaleSchemaMigration : AutoReversingMigration
{
    public override void Up()
    {
        Create.TableFor<TripleWhaleLogRecord>();
        Create.TableFor<TripleWhaleSyncQueueRecord>();
    }
}
