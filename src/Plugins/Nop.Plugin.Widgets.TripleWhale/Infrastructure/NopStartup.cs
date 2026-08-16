using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Widgets.TripleWhale.Infrastructure;

/// <summary>
/// Represents the plugin service configuration
/// </summary>
public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // API client and services are registered here in later phases.
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => 3000;
}
