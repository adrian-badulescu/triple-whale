using FluentValidation;
using Nop.Plugin.Widgets.TripleWhale.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.TripleWhale.Validators;

/// <summary>
/// Represents the configuration model validator
/// </summary>
public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
{
    public ConfigurationValidator(ILocalizationService localizationService)
    {
        RuleFor(model => model.ApiKey)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.TripleWhale.Fields.ApiKey.Required"));

        RuleFor(model => model.NumberOfDaysToKeepLogs)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.TripleWhale.Fields.NumberOfDaysToKeepLogs.MustBeGreaterThanZero"));
    }
}
