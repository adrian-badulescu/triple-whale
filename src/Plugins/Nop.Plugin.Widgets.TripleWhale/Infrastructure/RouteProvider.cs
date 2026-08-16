using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Widgets.TripleWhale.Infrastructure;

/// <summary>
/// Represents the plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(name: TripleWhaleDefaults.ConfigurationRouteName,
            pattern: "Admin/TripleWhale/Configure",
            defaults: new { controller = "TripleWhale", action = "Configure", area = AreaNames.ADMIN });
    }

    public int Priority => 0;
}
