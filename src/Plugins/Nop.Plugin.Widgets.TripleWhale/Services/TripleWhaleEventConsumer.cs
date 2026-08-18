using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Core.Http;
using Nop.Plugin.Widgets.TripleWhale.Domain;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Web.Framework.Events;

namespace Nop.Plugin.Widgets.TripleWhale.Services;

/// <summary>
/// Consumes nopCommerce events and enqueues Triple Whale sync items
/// </summary>
public class TripleWhaleEventConsumer :
    IConsumer<OrderPlacedEvent>,
    IConsumer<OrderPaidEvent>,
    IConsumer<OrderRefundedEvent>,
    IConsumer<CustomerRegisteredEvent>,
    IConsumer<EntityUpdatedEvent<Customer>>,
    IConsumer<EntityInsertedEvent<ShoppingCartItem>>,
    IConsumer<EmailSubscribedEvent>,
    IConsumer<ProductSearchEvent>,
    IConsumer<PageRenderingEvent>
{
    private readonly TripleWhaleSettings _settings;
    private readonly TripleWhaleEventBuilder _eventBuilder;
    private readonly TripleWhaleSyncService _syncService;
    private readonly TripleWhalePixelService _pixelService;
    private readonly ICustomerService _customerService;

    public TripleWhaleEventConsumer(
        TripleWhaleSettings settings,
        TripleWhaleEventBuilder eventBuilder,
        TripleWhaleSyncService syncService,
        TripleWhalePixelService pixelService,
        ICustomerService customerService)
    {
        _settings = settings;
        _eventBuilder = eventBuilder;
        _syncService = syncService;
        _pixelService = pixelService;
        _customerService = customerService;
    }

    /// <summary>
    /// Handle the order placed event
    /// </summary>
    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        if (eventMessage?.Order is not { } order)
            return;

        await SyncOrderAsync(order, "order.placed");
        await SendOrderEventAsync(order, "Purchase");
        await SendPurchasePixelEventAsync(order);
    }

    /// <summary>
    /// Handle the order paid event
    /// </summary>
    public async Task HandleEventAsync(OrderPaidEvent eventMessage)
    {
        if (eventMessage?.Order is not { } order)
            return;

        await SyncOrderAsync(order, "order.paid");
        await SendOrderEventAsync(order, "Purchase");
    }

    /// <summary>
    /// Handle the order refunded event
    /// </summary>
    public async Task HandleEventAsync(OrderRefundedEvent eventMessage)
    {
        if (eventMessage?.Order is not { } order)
            return;

        await SyncOrderAsync(order, "order.refunded");
        await SendOrderEventAsync(order, "Refund");
    }

    /// <summary>
    /// Handle the customer registered event
    /// </summary>
    public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
    {
        if (eventMessage?.Customer is not { } customer)
            return;

        if (_settings.EnablePixel)
        {
            await _pixelService.SendContactEventAsync(customer.Email);
            await _pixelService.SendSignUpEventAsync(customer.Email);
        }

        if (!_settings.EnableCustomerSync)
            return;

        var dto = await _eventBuilder.BuildCustomerAsync(customer);
        await _syncService.EnqueueAsync(TripleWhaleEntityType.Customer, customer.Id.ToString(), "customer.registered", dto);
    }

    /// <summary>
    /// Handle the customer updated event
    /// </summary>
    public async Task HandleEventAsync(EntityUpdatedEvent<Customer> eventMessage)
    {
        if (eventMessage?.Entity is not { } customer)
            return;

        if (!_settings.EnableCustomerSync)
            return;

        var dto = await _eventBuilder.BuildCustomerAsync(customer);
        await _syncService.EnqueueAsync(TripleWhaleEntityType.Customer, customer.Id.ToString(), "customer.updated", dto);
    }

    /// <summary>
    /// Handle the shopping cart item inserted event (add to cart)
    /// </summary>
    public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
    {
        if (eventMessage?.Entity is not { } item)
            return;

        if (!_settings.EnablePixel)
            return;

        await _pixelService.SendAddToCartEventAsync(item);
    }

    /// <summary>
    /// Handle the email subscribed event (newsletter)
    /// </summary>
    public async Task HandleEventAsync(EmailSubscribedEvent eventMessage)
    {
        if (eventMessage?.Subscription is not { } subscription)
            return;

        if (!_settings.EnablePixel)
            return;

        await _pixelService.SendContactEventAsync(subscription.Email);
        await _pixelService.SendLeadEventAsync(subscription.Email);
    }

    /// <summary>
    /// Handle the product search event
    /// </summary>
    public async Task HandleEventAsync(ProductSearchEvent eventMessage)
    {
        if (string.IsNullOrWhiteSpace(eventMessage?.SearchTerm))
            return;

        if (!_settings.EnablePixel)
            return;

        await _pixelService.SendSearchEventAsync(eventMessage.SearchTerm);
    }

    /// <summary>
    /// Handle the page rendering event (fires InitiateCheckout on the checkout page)
    /// </summary>
    public async Task HandleEventAsync(PageRenderingEvent eventMessage)
    {
        var routeName = eventMessage?.GetRouteName() ?? string.Empty;

        if (routeName != NopRouteNames.Standard.CHECKOUT && routeName != NopRouteNames.Standard.CHECKOUT_ONE_PAGE)
            return;

        if (!_settings.EnablePixel)
            return;

        await _pixelService.SendInitiateCheckoutEventAsync();
    }

    /// <summary>
    /// Sends a client-side Purchase pixel event with the order summary
    /// </summary>
    private async Task SendPurchasePixelEventAsync(Order order)
    {
        if (!_settings.EnablePixel)
            return;

        var purchaseData = await _eventBuilder.BuildPurchasePixelDataAsync(order);
        await _pixelService.SendPurchaseEventAsync(purchaseData);
    }

    /// <summary>
    /// Enqueues an order sync item when order sync is enabled
    /// </summary>
    private async Task SyncOrderAsync(Order order, string eventType)
    {
        if (!_settings.EnableOrderSync)
            return;

        var dto = await _eventBuilder.BuildOrderAsync(order);
        await _syncService.EnqueueAsync(TripleWhaleEntityType.Order, order.Id.ToString(), eventType, dto);
    }

    /// <summary>
    /// Enqueues a server-side conversion event when server-side events are enabled
    /// </summary>
    private async Task SendOrderEventAsync(Order order, string eventName)
    {
        if (!_settings.EnableServerSideEvents)
            return;

        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var offlineEvent = await _eventBuilder.BuildOfflineEventAsync(
            "order", eventName, customer?.Email, null, null, null, null, null, null);

        await _syncService.EnqueueAsync(TripleWhaleEntityType.Event, order.Id.ToString(), eventName, offlineEvent);
    }
}
