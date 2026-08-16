using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Widgets.TripleWhale.Domain;
using Nop.Plugin.Widgets.TripleWhale.Models;
using Nop.Plugin.Widgets.TripleWhale.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.TripleWhale.Controllers;

/// <summary>
/// Represents the plugin admin controller
/// </summary>
[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class TripleWhaleController : BasePluginController
{
    private readonly TripleWhaleSettings _settings;
    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly TripleWhaleHealthCheckService _healthCheckService;
    private readonly TripleWhaleSyncService _syncService;
    private readonly TripleWhaleEventBuilder _eventBuilder;
    private readonly IRepository<TripleWhaleLogRecord> _logRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Product> _productRepository;

    public TripleWhaleController(
        TripleWhaleSettings settings,
        ISettingService settingService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        TripleWhaleHealthCheckService healthCheckService,
        TripleWhaleSyncService syncService,
        TripleWhaleEventBuilder eventBuilder,
        IRepository<TripleWhaleLogRecord> logRepository,
        IRepository<Order> orderRepository,
        IRepository<Customer> customerRepository,
        IRepository<Product> productRepository)
    {
        _settings = settings;
        _settingService = settingService;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _healthCheckService = healthCheckService;
        _syncService = syncService;
        _eventBuilder = eventBuilder;
        _logRepository = logRepository;
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure()
    {
        var model = new ConfigurationModel
        {
            ApiKey = _settings.ApiKey,
            ShopUrl = _settings.ShopUrl,
            PixelSnippet = _settings.PixelSnippet,
            EnablePixel = _settings.EnablePixel,
            EnableServerSideEvents = _settings.EnableServerSideEvents,
            EnableOrderSync = _settings.EnableOrderSync,
            EnableCustomerSync = _settings.EnableCustomerSync,
            EnableProductSync = _settings.EnableProductSync
        };

        return View("~/Plugins/Widgets.TripleWhale/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_WIDGETS)]
    public async Task<IActionResult> Configure(ConfigurationModel model,
        string testConnection, string downloadLogs, string syncAll)
    {
        if (model is null)
            model = new ConfigurationModel();

        if (!string.IsNullOrEmpty(downloadLogs))
            return await DownloadLogsAsync();

        if (!string.IsNullOrEmpty(syncAll))
            return await SyncAllAsync();

        if (!string.IsNullOrEmpty(testConnection))
        {
            var check = await _healthCheckService.CheckAsync(model.ApiKey);
            if (check.Success)
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.TripleWhale.TestConnection.Success"));
            else
                _notificationService.ErrorNotification(string.Format(
                    await _localizationService.GetResourceAsync("Plugins.Widgets.TripleWhale.TestConnection.Failed"), check.Message));

            return await Configure();
        }

        if (!ModelState.IsValid)
            return await Configure();

        _settings.ApiKey = model.ApiKey;
        _settings.ShopUrl = model.ShopUrl;
        _settings.PixelSnippet = model.PixelSnippet;
        _settings.EnablePixel = model.EnablePixel;
        _settings.EnableServerSideEvents = model.EnableServerSideEvents;
        _settings.EnableOrderSync = model.EnableOrderSync;
        _settings.EnableCustomerSync = model.EnableCustomerSync;
        _settings.EnableProductSync = model.EnableProductSync;

        await _settingService.SaveSettingAsync(_settings);
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    private async Task<IActionResult> SyncAllAsync()
    {
        var count = 0;

        if (_settings.EnableOrderSync)
        {
            var orders = _orderRepository.Table.ToList();
            foreach (var order in orders)
            {
                var dto = await _eventBuilder.BuildOrderAsync(order);
                await _syncService.EnqueueAsync(TripleWhaleEntityType.Order, order.Id.ToString(), "order.sync", dto);
                count++;
            }
        }

        if (_settings.EnableCustomerSync)
        {
            var customers = _customerRepository.Table.ToList();
            foreach (var customer in customers)
            {
                var dto = await _eventBuilder.BuildCustomerAsync(customer);
                await _syncService.EnqueueAsync(TripleWhaleEntityType.Customer, customer.Id.ToString(), "customer.sync", dto);
                count++;
            }
        }

        if (_settings.EnableProductSync)
        {
            var products = _productRepository.Table.ToList();
            foreach (var product in products)
            {
                var dto = await _eventBuilder.BuildProductAsync(product);
                await _syncService.EnqueueAsync(TripleWhaleEntityType.Product, product.Id.ToString(), "product.sync", dto);
                count++;
            }
        }

        _notificationService.SuccessNotification(string.Format(
            await _localizationService.GetResourceAsync("Plugins.Widgets.TripleWhale.SyncAll.Success"), count));

        return await Configure();
    }

    private async Task<IActionResult> DownloadLogsAsync()
    {
        var logs = _logRepository.Table
            .OrderByDescending(log => log.CreatedDateUtc)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("LogType,Endpoint,Request,Response,Error,CreatedDateUtc");
        foreach (var log in logs)
            sb.AppendLine($"{log.LogType},{Escape(log.Endpoint)},{Escape(log.RequestMessage)},{Escape(log.ResponseMessage)},{Escape(log.ErrorMessage)},{log.CreatedDateUtc:O}");

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "triplewhale-logs.csv");
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value.Replace("\"", "\"\"").Replace(",", ";").Replace("\r", " ").Replace("\n", " ");
    }
}
