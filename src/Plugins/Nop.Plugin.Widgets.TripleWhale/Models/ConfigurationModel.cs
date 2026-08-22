using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.TripleWhale.Models;

/// <summary>
/// Represents the plugin configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Widgets.TripleWhale.Fields.ApiKey")]
    public string ApiKey { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.TripleWhale.Fields.ShopUrl")]
    public string ShopUrl { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.TripleWhale.Fields.PixelSnippet")]
    public string PixelSnippet { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.TripleWhale.Fields.EnablePixel")]
    public bool EnablePixel { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.TripleWhale.Fields.EnableServerSideEvents")]
    public bool EnableServerSideEvents { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.TripleWhale.Fields.EnableOrderSync")]
    public bool EnableOrderSync { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.TripleWhale.Fields.EnableCustomerSync")]
    public bool EnableCustomerSync { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.TripleWhale.Fields.EnableProductSync")]
    public bool EnableProductSync { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.TripleWhale.Fields.NumberOfDaysToKeepLogs")]
    public int NumberOfDaysToKeepLogs { get; set; }
}
