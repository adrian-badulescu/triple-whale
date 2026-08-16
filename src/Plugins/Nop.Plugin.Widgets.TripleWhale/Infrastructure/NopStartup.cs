using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.TripleWhale.Services;

namespace Nop.Plugin.Widgets.TripleWhale.Infrastructure;

/// <summary>
/// Represents the plugin service configuration
/// </summary>
public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<TripleWhaleEventBuilder>();
        services.AddScoped<TripleWhaleApiClient>();
        services.AddScoped<TripleWhaleLogService>();
        services.AddScoped<TripleWhaleSyncService>();
        services.AddScoped<TripleWhaleHealthCheckService>();
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}
